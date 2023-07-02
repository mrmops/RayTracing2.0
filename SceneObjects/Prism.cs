using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0.SceneObjects {
    public class Prism : ISceneObject {
        private List<Triangle> _triangles;
        private double _lenght;

        private Sphere _sphere;

        public Prism(float lenght, IMaterial material) {
            _lenght = lenght;
            Material = material;
            var vertexes = new List<Vector3>();
            for(int x = -1; x <= 1; x += 2) {
                for(int y = -1; y <= 1; y += 2) {
                    vertexes.Add(new Vector3(x, 0, y));
                }
            }

            vertexes.Add(new Vector3(0, lenght, 0));

            var vector3 = new Vector3(2, -1, 1);
            vertexes = vertexes.Select(x => x + vector3).ToList();

            CreateTriangle(vertexes);
            _sphere = new Sphere(vector3, lenght, null);
        }

        private void CreateTriangle(List<Vector3> vertexes) {
            const int index1 = 0;
            const int index2 = 1;
            const int index3 = 2;
            const int index4 = 3;
            const int index5 = 4;

            var a = (float)Math.Acos(1 / (1 + _lenght * _lenght));
            var xRot = Matrix4x4.CreateRotationX(a);
            var zRot = Matrix4x4.CreateRotationZ(a);

            _triangles = new List<Triangle>()
            {
                new Triangle(vertexes[index1], vertexes[index2], vertexes[index3], new Vector3(0, -1, 0)),
                new Triangle(vertexes[index2], vertexes[index3], vertexes[index4], new Vector3(0, -1, 0)),
                new Triangle(vertexes[index1], vertexes[index2], vertexes[index5], Vector3.Transform(new Vector3(1, 0, 0), zRot)),
                new Triangle(vertexes[index2], vertexes[index4], vertexes[index5], Vector3.Transform(new Vector3(0, 0, -1) , xRot)),
                new Triangle(vertexes[index4], vertexes[index3], vertexes[index5], Vector3.Transform(new Vector3(-1, 0, 0) , zRot)),
                new Triangle(vertexes[index3], vertexes[index1], vertexes[index5], Vector3.Transform(new Vector3(0, 0, +1) , xRot)),
            };
        }

        public IEnumerable<(float, Vector3)> FindIntersectedRayCoefficients(Ray ray) {
            if(!_sphere.FindIntersectedRayCoefficients(ray).Any())
                yield break;
            foreach(var triangle in _triangles) {
                if(triangle.FindIntersectedRayCoefficients(ray, out var t)) {
                    yield return (t, triangle.Normal);
                }
            }
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint) {
            throw new System.NotImplementedException();
        }

        public IMaterial Material { get; }
        public Vector3 Location => new Vector3(0, 0, 0);
    }
}