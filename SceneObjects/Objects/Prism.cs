// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Numerics;
// using RayTracing2._0.Infostructure;
// using RayTracing2._0.SceneObjects.Materials;
// using RayTracing2._0.SceneObjects.Objects.Models;
// using RayTracing2._0.SceneObjects.Objects.Spheres;
//
// namespace RayTracing2._0.SceneObjects.Objects;
//
// public class Prism : ISceneObject
// {
//     private List<Triangle> _triangles;
//     private double _lenght;
//
//     private Spheres.Sphere _sphere;
//
//     public Prism(float lenght, IMaterial material)
//     {
//         _lenght = lenght;
//         Material = material;
//         var vertexes = new List<Vector3>();
//         for (int x = -1; x <= 1; x += 2)
//         {
//             for (int y = -1; y <= 1; y += 2)
//             {
//                 vertexes.Add(new Vector3(x, 0, y));
//             }
//         }
//
//         vertexes.Add(new Vector3(0, lenght, 0));
//
//         var vector3 = new Vector3(2, -1, 1);
//         vertexes = vertexes.Select(x => x + vector3).ToList();
//
//         CreateTriangle(vertexes);
//         _sphere = new Sphere(vector3, lenght, null);
//     }
//
//     private void CreateTriangle(List<Vector3> vertexes)
//     {
//         const int index1 = 0;
//         const int index2 = 1;
//         const int index3 = 2;
//         const int index4 = 3;
//         const int index5 = 4;
//
//         var a = (float)Math.Acos(1 / (1 + _lenght * _lenght));
//         var xRot = Matrix4x4.CreateRotationX(a);
//         var zRot = Matrix4x4.CreateRotationZ(a);
//
//         var vertex1 = vertexes[index1];
//         var vertex2 = vertexes[index2];
//         var vertex4 = vertexes[index4];
//         var vertex3 = vertexes[index3];
//         var topVertex = vertexes[index5];
//         
//         _triangles = new List<Triangle>()
//         {
//             // Down
//             new(
//                 new TexturePoint(vertex1, topRight), 
//                 new TexturePoint(vertex2, topRight),  
//                 new TexturePoint(vertex3, bottomLeft), 
//                 forward, 
//                 Material
//             ),
//             new(
//                 new TexturePoint(vertex8, topRight), 
//                 new TexturePoint(vertex4, topRight),  
//                 new TexturePoint(vertex6, bottomLeft), 
//                 forward, 
//                 Material
//             ),
//             new(vertex1, vertex2, vertex3, new Vector3(0, -1, 0), Material),
//             new(vertex2, vertex3, vertex4, new Vector3(0, -1, 0), Material),
//             
//             
//             new(vertex1, vertex2, topVertex, Vector3.Transform(new Vector3(1, 0, 0), zRot),
//                 Material),
//             new(vertex2, vertex4, topVertex, Vector3.Transform(new Vector3(0, 0, -1), xRot),
//                 Material),
//             new(vertex4, vertex3, topVertex, Vector3.Transform(new Vector3(-1, 0, 0), zRot),
//                 Material),
//             new(vertex3, vertex1, topVertex, Vector3.Transform(new Vector3(0, 0, +1), xRot),
//                 Material),
//         };
//     }
//
//     public IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray)
//     {
//         if (!_sphere.FindIntersectedRayCoefficients(ray).Any())
//             yield break;
//         foreach (var triangle in _triangles)
//         {
//             if (triangle.FindIntersectedRayCoefficients(ray, out var t))
//             {
//                 yield return (t, triangle);
//             }
//         }
//     }
//
//     public IMaterial Material { get; }
// }