using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PreviewTest.Extensions
{
    public static class VectorExtension
    {
        public static Point3D ToPoint3D(in this Vector3D v)
        {
            return new Point3D(v.X, v.Y, v.Z);
        }

        public static Vector3D NormalizeTo(in this Vector3D v)
        {
            var result = v;
            result.Normalize();

            return result;
        }
    }
}
