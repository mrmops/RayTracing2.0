using System.Numerics;
using RayTracing2._0.SceneObjects;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Materials.Models;

namespace RayTracing2._0;

public struct IntersectionResult
{
    public Vector3 IntersectedPoint;
    public MaterialData MaterialData;

    public IntersectionResult(
        Vector3 intersectedPoint,
        MaterialData materialData
    )
    {
        IntersectedPoint = intersectedPoint;
        MaterialData = materialData;
    }
}