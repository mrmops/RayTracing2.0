using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Objects.Models;
using RayTracing2._0.SceneObjects.Objects.Spheres;

namespace RayTracing2._0.SceneObjects.Objects;

public class Cube : ISceneObject
{
    private Vector3 _center;
    private List<Triangle> _triangles;
    private readonly Sphere _container;
    public TextureMaterial Material { get; }
    
    public Cube(TextureMaterial material, int size, Vector3 center)
    {
        Material = material;
        _center = center;
        _container = new Sphere(center, size * Math.Sqrt(3), new EmptyMaterial());

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

        _triangles = CreateTriangle(vertexes);
    }

    private List<Triangle> CreateTriangle(List<Vector3> vertexes)
    {
        const int index1 = 0;
        const int index2 = 1;
        const int index3 = 2;
        const int index4 = 3;
        const int index5 = 4;
        const int index6 = 5;
        const int index7 = 6;
        const int index8 = 7;

        var left = new Vector3(-1, 0, 0);
        var right = new Vector3(1, 0, 0);

        var top = new Vector3(0, 1, 0);
        var down = new Vector3(0, -1, 0);
        
        var forward = new Vector3(0, 0, 1);
        var backward = new Vector3(0, 0, -1);

        var topLeft = new Vector2(-1, 1);
        var topRight = new Vector2(1, 1);
        var bottomLeft = new Vector2(-1, -1);
        var bottomRight = new Vector2(1, -1);

        var vertex1 = vertexes[index1];
        var vertex2 = vertexes[index2];
        var vertex3 = vertexes[index3];
        var vertex4 = vertexes[index4];
        var vertex5 = vertexes[index5];
        var vertex6 = vertexes[index6];
        var vertex7 = vertexes[index7];
        var vertex8 = vertexes[index8];
        
        
        return new List<Triangle>()
        {
            // Left
            new(
                new TexturePoint(vertex1, topLeft), 
                new TexturePoint(vertex3, topRight),  
                new TexturePoint(vertex2, bottomLeft), 
                left, 
                Material
            ),
            new(
                new TexturePoint(vertex4, bottomRight), 
                new TexturePoint(vertex3, topRight),  
                new TexturePoint(vertex2, bottomLeft), 
                left, 
                Material
            ),
            
            // Top
            new(
                new TexturePoint(vertex3, topLeft), 
                new TexturePoint(vertex4, topRight),  
                new TexturePoint(vertex7, bottomLeft), 
                top, 
                Material
            ),
            new(
                new TexturePoint(vertex8, bottomRight), 
                new TexturePoint(vertex4, topRight),  
                new TexturePoint(vertex7, bottomLeft), 
                top, 
                Material
            ),
            
            // Right
            new(
                new TexturePoint(vertex7, topLeft), 
                new TexturePoint(vertex5, topRight),  
                new TexturePoint(vertex8, bottomLeft), 
                right, 
                Material
            ),
            new(
                new TexturePoint(vertex6, bottomRight), 
                new TexturePoint(vertex5, topRight),  
                new TexturePoint(vertex8, bottomLeft), 
                right, 
                Material
            ),
            
            // Down
            new(
                new TexturePoint(vertex5, topLeft), 
                new TexturePoint(vertex1, topRight),  
                new TexturePoint(vertex6, bottomLeft), 
                down, 
                Material
            ),
            new(
                new TexturePoint(vertex2, bottomRight), 
                new TexturePoint(vertex1, topRight),  
                new TexturePoint(vertex6, bottomLeft), 
                down, 
                Material
            ),
            
            // Forward
            new(
                new TexturePoint(vertex2, topLeft), 
                new TexturePoint(vertex4, topRight),  
                new TexturePoint(vertex6, bottomLeft), 
                forward, 
                Material
            ),
            new(
                new TexturePoint(vertex8, bottomRight), 
                new TexturePoint(vertex4, topRight),  
                new TexturePoint(vertex6, bottomLeft), 
                forward, 
                Material
            ),
            
            // Backward
            new(
                new TexturePoint(vertex1, topLeft), 
                new TexturePoint(vertex5, topRight),  
                new TexturePoint(vertex3, bottomLeft), 
                backward, 
                Material
            ),
            new(
                new TexturePoint(vertex7, bottomRight), 
                new TexturePoint(vertex5, topRight),  
                new TexturePoint(vertex3, bottomLeft), 
                backward, 
                Material
            ),
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
}