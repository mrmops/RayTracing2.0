using System.Numerics;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0.SceneObjects;

public interface IRenderObject
{
    MaterialCoefficientData GetMaterialByIntersection(Vector3 intersectionPoint);
}