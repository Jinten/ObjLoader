using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace ObjLoader
{
    public static class ObjFileLoader
    {
        class VertexIndex
        {
            public int Position { get; set; } = -1;
            public int Normal { get; set; } = -1;
            public int UV { get; set; } = -1;
        }

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

            return LoadInternal(objHandle, in data);
        }

        public static bool LoadAsync(IObjHandle handle, string path, Action<IObjHandle> complete, Action<int, int> loadProgress = null, Action<int, int> constructProgress = null)
        {
            if (ValidateError(handle, path) == false)
            {
                return false;
            }

            var objHandle = handle as ObjHandle;

            Task.Run(async () =>
            {
                string data = string.Empty;
                using (var reader = new StreamReader(path))
                {
                    data = await reader.ReadToEndAsync();
                }

                LoadInternal(objHandle, in data, loadProgress, constructProgress);
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

        static bool LoadInternal(ObjHandle objHandle, in string data, Action<int, int> parseProgress = null, Action<int, int> constructProgress = null)
        {
            var positionList = new List<Vector3D>();
            var normalList = new List<Vector3D>();
            var uvList = new List<Vector>();

            var vertexIndexList = new List<VertexIndex>();
            var clusterList = new List<Cluster>();
            var materialList = new List<Material>();

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
                        ++strIndex; // skip 'v'
                        switch (data[strIndex])
                        {
                            case ' ':   // vertex position:
                            case '\t':
                                {
                                    // skip to until not white splace character.
                                    strIndex = SkipToValidCharacter(strIndex, data);

                                    var pos = new Vector3D();
                                    pos.X = ReadValue(ref strIndex, data);
                                    pos.Y = ReadValue(ref strIndex, data);
                                    pos.Z = ReadValue(ref strIndex, data);
                                    positionList.Add(pos);
                                }
                                break;
                            case 'n':   // vertex normal
                                {
                                    ++strIndex; // skip 'n'

                                    // skip to until not white splace character.
                                    strIndex = SkipToValidCharacter(strIndex, data);

                                    var normal = new Vector3D();
                                    normal.X = ReadValue(ref strIndex, data);
                                    normal.Y = ReadValue(ref strIndex, data);
                                    normal.Z = ReadValue(ref strIndex, data);
                                    normalList.Add(normal);
                                }
                                break;
                            case 't':   // vertex uv
                                {
                                    ++strIndex; // skip 't'

                                    // skip to until not white splace character.
                                    strIndex = SkipToValidCharacter(strIndex, data);

                                    var uv = new Vector();
                                    uv.X = ReadValue(ref strIndex, data);
                                    uv.Y = ReadValue(ref strIndex, data);
                                    uvList.Add(uv);
                                }
                                break;
                            default:
                                throw new InvalidDataException("Invalid vertex data.");
                        }
                        break;
                    case 'f':
                        {
                            ++strIndex; // skip 'f'

                            // skip to until not white splace character.
                            strIndex = SkipToValidCharacter(strIndex, data);

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

                                if (EndOfLine(strIndex, data) == false && data[strIndex] == '/')
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
                                        if (EndOfLine(strIndex, data) == false && data[strIndex] == '/')
                                        {
                                            strIndex += 1; // skip '/'    
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

                            int indexCount;
                            if (vIndexList.Count == 3)
                            {
                                vertexIndexList.AddRange(vIndexList);
                                indexCount = 3;
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

                                indexCount = count * 3;
                            }

                            if (clusterList.Any())
                            {
                                clusterList.Last().IndexCount += indexCount;
                            }

                            strIndex = SkipToEndOfLine(strIndex, data);
                        }
                        break;
                    case 'u': // usemtl
                        {
                            strIndex += 6; // skip "usemtl"
                            strIndex = SkipToValidCharacter(strIndex, data);

                            var materialName = ReadSequentialString(ref strIndex, data);

                            var masterMaterial = materialList.FirstOrDefault(arg => arg.Name == materialName);
                            if(masterMaterial == null)
                            {
                                masterMaterial = new Material(materialList.Count, materialName);
                                materialList.Add(masterMaterial);
                            }
                            clusterList.Add(new Cluster(masterMaterial.Index, vertexIndexList.Count));
                        }
                        break;
                    case 'm': // mtllib
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

            int vCount = positionList.Count;
            int nCount = normalList.Count;
            int uvCount = uvList.Count;

            constructProgress?.Invoke(vertexIndexList.Count, 0);

            var vertices = new List<Vertex>();
            var indices = new List<int>();
            var vertexIndexCacheDict = new Dictionary<string, int>();

            for (int i = 0; i < vertexIndexList.Count; ++i)
            {
                var vIndex = vertexIndexList[i];

                int posIndex = GetRequiredIndex(vIndex.Position, vCount);
                int normalIndex = GetOptionalIndex(vIndex.Normal, nCount);
                int uvIndex = GetOptionalIndex(vIndex.UV, uvCount);

                string key = uvIndex.ToString() + normalIndex.ToString() + posIndex.ToString();

                int cacheIndex;
                if (vertexIndexCacheDict.TryGetValue(key, out cacheIndex))
                {
                    indices.Add(cacheIndex);
                }
                else
                {
                    var vertex = new Vertex()
                    {
                        Position = positionList[posIndex],
                        Normal = normalIndex == -1 ? new Vector3D(0, 0, 0) : normalList[normalIndex],
                        UV = uvIndex == -1 ? new Vector(-1, -1) : uvList[uvIndex]
                    };

                    vertices.Add(vertex);
                    cacheIndex = vertices.Count - 1;

                    indices.Add(cacheIndex);
                    vertexIndexCacheDict.Add(key, cacheIndex);
                }

                constructProgress?.Invoke(vertexIndexList.Count, i);
            }

            objHandle.Vertices = vertices.ToArray();
            objHandle.Indices = indices.ToArray();
            objHandle.Clusters = clusterList.ToArray();
            objHandle.Materials = materialList.ToArray();

            return true;
        }

        static int GetRequiredIndex(int index, int maxCount)
        {
            if (index >= 0)
            {
                return index;
            }

            if (index == -1)
            {
                // refer to the last index.
                return maxCount - 1;
            }

            // relatively refers to the end of the list.
            return maxCount + index;
        }

        static int GetOptionalIndex(int index, int maxCount)
        {
            if (index >= 0)
            {
                return index;
            }

            if (index == -1)
            {
                // unused.
                return -1;
            }

            // relatively refers to the end of the list.
            return maxCount + index;
        }

        static string ReadSequentialString(ref int index, in string data)
        {
            string src = string.Empty;
            while(EndOfLine(index, data) == false && char.IsWhiteSpace(data[index]) == false)
            {
                src += data[index];
                ++index;
            }

            return src;
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
                // skip for next character.
                index = SkipToValidCharacter(index, data);
            }

            if(src.Equals("-1.#IND00"))
            {
                return double.NaN;
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

        static int SkipToValidCharacter(int index, string data)
        {
            while (char.IsWhiteSpace(data[index]))
            {
                ++index;
            }

            return index;
        }

        static int SkipToEndOfLine(int index, string data)
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
