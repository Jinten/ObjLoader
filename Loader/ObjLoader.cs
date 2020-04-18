using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Loader
{
    public static class ObjLoader
    {
        public static IObjHandle CreateHandle()
        {
            var handle = new ObjHandle();

            return handle;
        }

        public static bool Load(IObjHandle handle, in string path)
        {
            if (ValidateError(handle, path) == false)
            {
                return false;
            }

            string data = string.Empty;
            using (var reader = new StreamReader(path))
            {
                data = reader.ReadToEnd();
            }

            var objHandle = handle as ObjHandle;

            return LoadInternal(objHandle, data);
        }

        public static bool LoadAsync(IObjHandle handle, string path, Action<IObjHandle> complete, Action<int, int> loadProgress = null, Action<int, int> constructProgress = null)
        {
            if (ValidateError(handle, path) == false)
            {
                return false;
            }

            var objHandle = handle as ObjHandle;

            Task.Run(async ()=>
            {
                string data = string.Empty;
                using (var reader = new StreamReader(path))
                {
                    data = await reader.ReadToEndAsync();
                }
                
                LoadInternal(objHandle, data, loadProgress, constructProgress);
                complete(objHandle);
            });

            return true;
        }

        static bool ValidateError(IObjHandle handle, in string path)
        {
            if (File.Exists(path) == false)
            {
                Console.WriteLine($"File not exists. Path = {path}");
                return false;
            }

            var objHandle = handle as ObjHandle;
            if (objHandle == null)
            {
                throw new InvalidProgramException("Cannot cast IObjHandle to ObjHandle.");
            }

            return true;
        }

        static bool LoadInternal(ObjHandle objHandle, string data, Action<int, int> parseProgress = null, Action<int, int> constructProgress = null)
        {
            var positionList = new List<Vector3D>();
            var normalList = new List<Vector3D>();
            var uvList = new List<Vector>();

            var vertexIndexList = new List<VertexIndex>();

            parseProgress?.Invoke(data.Length, 0);

            int strIndex = 0;
            while (strIndex < data.Length)
            {
                char c = data[strIndex];
                switch (c)
                {
                    case '#':
                        strIndex = SkipToEndOfLine(strIndex, data);
                        break;
                    case 'v':
                        switch (data[strIndex + 1])
                        {
                            case ' ':   // vertex position:
                                {
                                    strIndex += 2; // skip 'v' and ' '.

                                    var pos = new Vector3D();
                                    pos.X = ReadValue(ref strIndex, data);
                                    pos.Y = ReadValue(ref strIndex, data);
                                    pos.Z = ReadValue(ref strIndex, data);
                                    positionList.Add(pos);

                                    strIndex = SkipToEndOfLine(strIndex, data);
                                }
                                break;
                            case 'n':   // vertex normal
                                {
                                    strIndex += 3; // skip 'v', 'n' and ' '.

                                    var normal = new Vector3D();
                                    normal.X = ReadValue(ref strIndex, data);
                                    normal.Y = ReadValue(ref strIndex, data);
                                    normal.Z = ReadValue(ref strIndex, data);
                                    normalList.Add(normal);

                                    strIndex = SkipToEndOfLine(strIndex, data);
                                }
                                break;
                            case 't':   // vertex uv
                                {
                                    strIndex += 3; // skip 'v', 't' and ' '.

                                    var uv = new Vector();
                                    uv.X = ReadValue(ref strIndex, data);
                                    uv.Y = ReadValue(ref strIndex, data);
                                    uvList.Add(uv);

                                    strIndex = SkipToEndOfLine(strIndex, data);
                                }
                                break;
                            default:
                                throw new InvalidDataException("Invalid vertex data.");
                        }
                        break;
                    case 'f':
                        {
                            strIndex += 2; // skip 'f' and ' '.

                            var vIndexList = new List<VertexIndex>();

                            while (EndOfLine(strIndex, data) == false)
                            {
                                if (data[strIndex] == ' ')
                                {
                                    ++strIndex;
                                    continue;
                                }

                                var vIndex = new VertexIndex();
                                vIndex.Position = ReadFaceIndex(ref strIndex, data);
                                if (vIndex.Position > 0)
                                {
                                    // plus index start from 1, so need to decrement just -1.
                                    --vIndex.Position;
                                }

                                if (data[strIndex] == '/')
                                {
                                    if (data[strIndex + 1] == '/')
                                    {
                                        // combination vertex index and normal index.
                                        strIndex += 2; // skip '/' and '/'
                                        vIndex.Normal = ReadFaceIndex(ref strIndex, data);
                                        if (vIndex.Normal > 0)
                                        {
                                            // plus index start from 1, so need to decrement just -1.
                                            --vIndex.Normal;
                                        }
                                    }
                                    else
                                    {
                                        // texture uv
                                        strIndex += 1; // skip '/'
                                        vIndex.UV = ReadFaceIndex(ref strIndex, data) - 1;

                                        // end or exists normal index.
                                        if (data[strIndex] == '/' && EndOfLine(strIndex, data) == false)
                                        {
                                            vIndex.Normal = ReadFaceIndex(ref strIndex, data) - 1;
                                        }
                                        else
                                        {
                                            // just only vertex index and texture uv index.
                                            ++strIndex;
                                        }
                                    }
                                }

                                vIndexList.Add(vIndex);
                            }

                            if (vIndexList.Count == 3)
                            {
                                vertexIndexList.AddRange(vIndexList);
                            }
                            else
                            {
                                // declarated polygons, not declarated only just by triangle.
                                int count = vIndexList.Count / 2 + vIndexList.Count % 2;
                                for (int i = 0; i < count; ++i)
                                {
                                    vertexIndexList.Add(vIndexList[0]);
                                    vertexIndexList.Add(vIndexList[i + 1]);
                                    vertexIndexList.Add(vIndexList[i + 2]);
                                }
                            }

                            strIndex = SkipToEndOfLine(strIndex, data);
                        }
                        break;
                    case 'm': // mtllib
                    case 'u': // usemtl
                    case 'g':
                    case 'o':
                    case 's':
                        // not supported.
                        strIndex = SkipToEndOfLine(strIndex, data);
                        break;
                    default:
                        ++strIndex;
                        break;
                }

                parseProgress?.Invoke(data.Length, strIndex);
            }

            int vCount = positionList.Count();
            int nCount = normalList.Count();
            var vertices = new List<Vertex>();

            constructProgress?.Invoke(vertexIndexList.Count, 0);

            for (int i = 0; i < vertexIndexList.Count; ++i)
            {
                var vIndex = vertexIndexList[i];

                var vertex = new Vertex()
                {
                    Position = positionList[GetIndex(vIndex.Position, vCount)],
                    Normal = vIndex.Normal == -1 ? new Vector3D(0, 0, 0) : normalList[GetIndex(vIndex.Normal, nCount)],
                    UV = vIndex.UV == -1 ? new Vector(-1, -1) : uvList[vIndex.UV]
                };

                vertices.Add(vertex);

                constructProgress?.Invoke(vertexIndexList.Count, i);
            }

            objHandle.Vertices = vertices.ToArray();

            return true;
        }

        static int GetIndex(int index, int maxCount)
        {
            if (index >= 0)
            {
                return index;
            }

            if (index == -1)
            {
                return maxCount - 1;
            }

            return maxCount + index;
        }

        static int ReadFaceIndex(ref int index, in string data)
        {
            int test = index;
            string src = string.Empty;
            while (EndOfLine(index, data) == false && char.IsWhiteSpace(data[index]) == false && data[index] != '/')
            {
                src += data[index];
                ++index;
            }

            return int.Parse(src);
        }

        static double ReadValue(ref int index, in string data)
        {
            string src = string.Empty;
            while (EndOfLine(index, data) == false && char.IsWhiteSpace(data[index]) == false)
            {
                src += data[index];
                ++index;
            }

            if (data[index] != '\n')
            {
                ++index; // skip for next character.
            }

            return double.Parse(src);
        }

        static bool EndOfLine(int index, string data)
        {
            if (index >= data.Length)
            {
                return true;
            }
            return data[index] == '\r' || data[index] == '\n' || data[index] == '\0';
        }

        static int SkipToEndOfLine(int index, in string data)
        {
            if (index >= data.Length)
            {
                return index;
            }

            while (data[index] != '\n')
            {
                ++index;
            }
            ++index; // skip for next character.

            return index;
        }
    }
}
