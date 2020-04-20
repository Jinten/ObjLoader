using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PreviewTest.Extensions
{
    public static class VectorExtension
    {
        static readonly Vector3D Zero = new Vector3D(0, 0, 0);

        public static Point ToPoint(in this Vector v)
        {
            return new Point(v.X, v.Y);
        }

        public static Vector NormalizeTo(in this Vector v)
        {
            var result = v;
            result.Normalize();

            return result;
        }

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

        public static Vector3D TransfromNormal(in this Vector3D v, in Quaternion rotation)
        {
            if(rotation.Equals(Quaternion.Identity))
            {
                return v;
            }
            var transform = new RotateTransform3D(new QuaternionRotation3D(rotation));
            return transform.Transform(v);
        }

        public static Quaternion MakeQuaternionBetweenVectors(in this Vector3D from, in Vector3D to)
        {
            var axis = Vector3D.CrossProduct(from, to);
            if (Vector3D.Equals(axis, Zero))
            {
                return Quaternion.Identity;
            }

            return new Quaternion(axis, Vector3D.AngleBetween(from, to));
        }
    }
}
