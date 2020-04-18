using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Loader
{
    public class Vertex
    {
        public Vector3D Position { get; set; } = new Vector3D();
        public Vector3D Normal { get; set; } = new Vector3D();
        public Vector UV { get; set; } = new Vector();
    }

    public class VertexIndex
    {
        public int Position { get; set; } = -1;
        public int Normal { get; set; } = -1;
        public int UV { get; set; } = -1;
    }

    internal class ObjHandle : IObjHandle
    {
        public Vertex[] Vertices { get; set; } = null;
        public int[] Indices { get; set; } = null;
    }
}
