using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Loader
{
    public interface IVertex
    {
        Vector3D Position { get; }
        Vector3D Normal { get; }
        Vector UV { get; }
    }

    public interface IMaterial
    {
        string Name { get; }
    }

    public interface ICluster
    {
        int MaterialIndex { get; }
        int StartIndex { get; }
        int IndexCount { get; }
    }

    public interface IObjHandle
    {
        IVertex[] Vertices { get; }
        int[] Indices { get; }
        ICluster[] Clusters { get; }
        IMaterial[] Materials { get; }
    }
}
