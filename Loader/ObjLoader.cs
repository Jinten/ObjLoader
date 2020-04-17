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

            string data = string.Empty;

            using (var reader = new StreamReader(path))
            {
                data = reader.ReadToEnd();
            }

            var positionList = new List<Vector3D>();
            var normalList = new List<Vector3D>();
            var uvList = new List<Vector>();

            var vertexIndexList = new List<VertexIndex>();

            int index = 0;
            while (index < data.Length)
            {
                char c = data[index];
                switch (c)
                {
                    case '#':
                        index = SkipToEndOfLine(index, data);
                        break;
                    case 'v':
                        switch (data[index + 1])
                        {
                            case ' ':   // vertex position:
                                {
                                    index += 2; // skip 'v' and ' '.

                                    var pos = new Vector3D();
                                    pos.X = ReadValue(ref index, data);
                                    pos.Y = ReadValue(ref index, data);
                                    pos.Z = ReadValue(ref index, data);
                                    positionList.Add(pos);

                                    index = SkipToEndOfLine(index, data);
                                }
                                break;
                            case 'n':   // vertex normal
                                {
                                    index += 3; // skip 'v', 'n' and ' '.

                                    var normal = new Vector3D();
                                    normal.X = ReadValue(ref index, data);
                                    normal.Y = ReadValue(ref index, data);
                                    normal.Z = ReadValue(ref index, data);
                                    normalList.Add(normal);

                                    index = SkipToEndOfLine(index, data);
                                }
                                break;
                            case 't':   // vertex uv
                                {
                                    index += 3; // skip 'v', 't' and ' '.

                                    var uv = new Vector();
                                    uv.X = ReadValue(ref index, data);
                                    uv.Y = ReadValue(ref index, data);
                                    uvList.Add(uv);

                                    index = SkipToEndOfLine(index, data);
                                }
                                break;
                            default:
                                throw new InvalidDataException("Invalid vertex data.");
                        }
                        break;
                    case 'f':
                        {
                            index += 2; // skip 'f' and ' '.

                            var vIndexList = new List<VertexIndex>();

                            while (EndOfLine(index, data) == false)
                            {
                                if (data[index] == ' ')
                                {
                                    ++index;
                                    continue;
                                }

                                var vIndex = new VertexIndex();
                                vIndex.Position = ReadFaceIndex(ref index, data) - 1;

                                if (data[index] == '/')
                                {
                                    if (data[index + 1] == '/')
                                    {
                                        // combination vertex index and normal index.
                                        index += 2; // skip '/' and '/'
                                        vIndex.Normal = ReadFaceIndex(ref index, data) - 1;
                                    }
                                    else
                                    {
                                        // texture uv
                                        index += 1; // skip '/'
                                        vIndex.UV = ReadFaceIndex(ref index, data) - 1;

                                        ++index;

                                        // end or exists normal index.
                                        if (EndOfLine(index, data) == false)
                                        {
                                            vIndex.Normal = ReadFaceIndex(ref index, data) - 1;
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

                            index = SkipToEndOfLine(index, data);
                        }
                        break;
                    case 'm': // mtllib
                    case 'u': // usemtl
                    case 'g':
                    case 'o':
                    case 's':
                        // not supported.
                        index = SkipToEndOfLine(index, data);
                        break;
                    default:
                        ++index;
                        break;
                }
            }

            var vertices = new List<Vertex>();
            foreach(var vIndex in vertexIndexList)
            {
                var vertex = new Vertex();
                vertex.Position = positionList[vIndex.Position];
                vertex.Normal = vIndex.Normal == -1 ? new Vector3D(0, 0, 0) : normalList[vIndex.Normal];
                vertex.UV = vIndex.UV == -1 ? new Vector(-1, -1) : uvList[vIndex.UV];

                vertices.Add(vertex);
            }

            objHandle.Vertices = vertices.ToArray();

            return true;
        }

        static int ReadFaceIndex(ref int index, in string data)
        {
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
