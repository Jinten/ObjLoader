using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace ObjLoader
{
    internal class Vertex : IVertex
    {
        public Vector3D Position { get; set; } = new Vector3D();
        public Vector3D Normal { get; set; } = new Vector3D();
        public Vector UV { get; set; } = new Vector();
    }

    internal class Material : IMaterial
    {
        public int Index { get; }
        public string Name { get; }

        public Material(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }

    internal class Cluster : ICluster
    {
        public int MaterialIndex { get; } = -1;
        public int StartIndex { get; }
        public int IndexCount { get; set; } = -1;

        public Cluster(int masterIndex, int startIndex)
        {
            MaterialIndex = masterIndex;
            StartIndex = startIndex;
        }
    }

    internal class ObjHandle : IObjHandle
    {
        public IVertex[] Vertices { get; set; } = null;
        public int[] Indices { get; set; } = null;
        public ICluster[] Clusters { get; set; } = null;
        public IMaterial[] Materials { get; set; } = null;
    }
}
