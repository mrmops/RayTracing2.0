using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Objects.Spheres;

namespace RayTracing2._0.SceneObjects.Objects;

public class Cube : ISceneObject
{
    private Vector3 center;
    private List<Triangle> _triangles;
    private readonly Sphere _container;

    public Cube(IMaterial material, int size, Vector3 center)
    {
        Material = material;
        this.center = center;
        _container = new Spheres.Sphere(center, size * Math.Sqrt(3), new EmptyMaterial());

        var vertexes = new List<Vector3>();
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    vertexes.Add(new Vector3(x, y, z) / 2 * size + center);
                }
            }
        }

        CreateTriangle(vertexes);
    }

    private void CreateTriangle(List<Vector3> vertexes)
    {
        const int index1 = 0;
        const int index2 = 1;
        const int index3 = 2;
        const int index4 = 3;
        const int index5 = 4;
        const int index6 = 5;
        const int index7 = 6;
        const int index8 = 7;

        _triangles = new List<Triangle>()
        {
            new(vertexes[index1], vertexes[index2], vertexes[index3], new Vector3(-1, 0, 0), Material),
            new(vertexes[index2], vertexes[index3], vertexes[index4], new Vector3(-1, 0, 0), Material),
            new(vertexes[index3], vertexes[index4], vertexes[index7], new Vector3(0, 1, 0), Material),
            new(vertexes[index8], vertexes[index4], vertexes[index7], new Vector3(0, 1, 0), Material),
            new(vertexes[index8], vertexes[index5], vertexes[index7], new Vector3(1, 0, 0), Material),
            new(vertexes[index8], vertexes[index5], vertexes[index6], new Vector3(1, 0, 0), Material),
            new(vertexes[index1], vertexes[index5], vertexes[index6], new Vector3(0, -1, 0), Material),
            new(vertexes[index1], vertexes[index2], vertexes[index6], new Vector3(0, -1, 0), Material),
            new(vertexes[index4], vertexes[index2], vertexes[index6], new Vector3(0, 0, 1), Material),
            new(vertexes[index4], vertexes[index8], vertexes[index6], new Vector3(0, 0, 1), Material),
            new(vertexes[index1], vertexes[index5], vertexes[index3], new Vector3(0, 0, -1), Material),
            new(vertexes[index7], vertexes[index5], vertexes[index3], new Vector3(0, 0, -1), Material),
        };
    }

    public IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray)
    {
        if (!_container.FindIntersectedRayCoefficients(ray).Any())
        {
            yield break;
        }


        foreach (var triangle in _triangles)
        {
            if (triangle.FindIntersectedRayCoefficients(ray, out var t))
            {
                yield return (t, triangle);
            }
        }
    }

    /*private Vector3 NewRandomNormal(Vector3 normal)
    {
        var x = random.NextDouble() - 0.5;
        var y = random.NextDouble() - 0.5;
        var z = random.NextDouble() - 0.5;

        return (normal + new Vector3(x, y, z)).Normalized();
    }*/

    public IMaterial Material { get; }
    public Vector3 Location { get; } = new(0, 0, 0);
}