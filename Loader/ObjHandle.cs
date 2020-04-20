using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Loader
{
    internal class Vertex : IVertex
    {
        public Vector3D Position { get; set; } = new Vector3D();
        public Vector3D Normal { get; set; } = new Vector3D();
        public Vector UV { get; set; } = new Vector();
    }

    internal class MasterMaterial : IMasterMaterial
    {
        public int Index { get; }
        public string Name { get; }

        public MasterMaterial(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }

    internal class Material : IMaterial
    {
        public int MasterIndex { get; } = -1;
        public int StartIndex { get; }
        public int IndexCount { get; set; } = -1;

        public Material(int masterIndex, int startIndex)
        {
            MasterIndex = masterIndex;
            StartIndex = startIndex;
        }
    }

    internal class ObjHandle : IObjHandle
    {
        public IVertex[] Vertices { get; set; } = null;
        public int[] Indices { get; set; } = null;
        public IMaterial[] Materials { get; set; } = null;
        public IMasterMaterial[] MasterMaterials { get; set; } = null;
    }
}
