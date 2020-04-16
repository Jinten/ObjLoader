using PreviewTest.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PreviewTest.Utilities
{
    internal static class GeometryMaker
    {
        internal static GeometryModel3D MakeCube3D(Brush brush = null)
        {
            if (brush == null)
            {
                brush = Brushes.LightGreen;
            }

            // cube's triangles 12 planes
            var vertices = new[]
            {
                    new Point3D(-1,  1,  1),
                    new Point3D(-1, -1,  1),
                    new Point3D( 1, -1,  1),
                    new Point3D( 1,  1,  1),
                    new Point3D(-1,  1, -1),
                    new Point3D(-1, -1, -1),
                    new Point3D( 1, -1, -1),
                    new Point3D( 1,  1, -1)
            };

            // to get vertex from face index
            Point3D[] face(int i1, int i2, int i3, int i4) => new[] { i1, i2, i3, i1, i3, i4 }.Select(x => vertices[x]).ToArray();

            // actual vertex points
            Point3D[] positions = new[]
            {
                    face(0, 1, 2, 3),
                    face(3, 2, 6, 7),
                    face(7, 6, 5, 4),
                    face(4, 5, 1, 0),
                    face(0, 3, 7, 4),
                    face(5, 6, 2, 1),
            }.SelectMany(x => x).ToArray();

            return new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D() { Positions = new Point3DCollection(positions) },
                Material = new DiffuseMaterial(brush),
            };
        }

        internal static GeometryModel3D MakeLine3D(Vector3D start, Vector3D end, Brush brush, double scale = 0.01)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            var length = (end - start).Length;

            var positions = new List<Point3D>();
            positions.Add(new Point3D(-scale, +scale, 0));
            positions.Add(new Point3D(-scale, +scale, +length));
            positions.Add(new Point3D(+scale, +scale, +length));
            positions.Add(new Point3D(+scale, +scale, 0));
            positions.Add(new Point3D(-scale, -scale, 0));
            positions.Add(new Point3D(-scale, -scale, +length));
            positions.Add(new Point3D(+scale, -scale, +length));
            positions.Add(new Point3D(+scale, -scale, 0));

            var segment = end - start;
            var defaultAxis = new Vector3D(0, 0, length);

            var rot = Vector3D.AngleBetween(defaultAxis, segment);
            var rotAxis = Vector3D.CrossProduct(defaultAxis.NormalizeTo(), segment.NormalizeTo());

            var matRot = new Matrix3D();
            if (rotAxis.Length > 0.0)
            {
                matRot.Rotate(new Quaternion(rotAxis, rot));
            }

            foreach(var pos in positions)
            {
                mesh.Positions.Add(matRot.Transform(pos) + start);
            }

            // top
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);

            // bottom
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);

            // left
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(1);

            // right
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(2);

            mesh.Freeze();
            
            var diffuse = new DiffuseMaterial(Brushes.Black);
            var emissive = new EmissiveMaterial(brush);
            diffuse.Freeze();
            emissive.Freeze();

            var material = new MaterialGroup();
            material.Children.Add(diffuse);
            material.Children.Add(emissive);
            material.Freeze();

            return new GeometryModel3D(mesh, material)
            {
                BackMaterial = material
            };
        }
    }
}
