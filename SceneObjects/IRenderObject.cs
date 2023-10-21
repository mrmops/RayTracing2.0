using System.Numerics;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Materials.Models;

namespace RayTracing2._0.SceneObjects;

public interface IRenderObject
{
    MaterialData GetMaterialByIntersection(Vector3 intersectionPoint);
}