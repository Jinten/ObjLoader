using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PreviewTest.Extensions
{
    public static class Point3DExtension
    {
        public static Vector3D ToVector3D(in this Point3D p)
        {
            return new Vector3D(p.X, p.Y, p.Z);
        }

        public static Vector3D NormalizeToVector3D(in this Point3D p)
        {
            var v = new Vector3D(p.X, p.Y, p.Z);
            v.Normalize();

            return v;
        }

        public static double Length(in this Point3D p)
        {
            var v = new Vector3D(p.X, p.Y, p.Z);
            return v.Length;
        }
    }
}
