using System.Numerics;
using RayTracing2._0.SceneObjects;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Materials.Models;

namespace RayTracing2._0;

public struct IntersectionResult
{
    public ISceneObject IntersectedObject;
    public Vector3 IntersectedPoint;
    public MaterialData MaterialData;

    public IntersectionResult(
        ISceneObject intersectedObject,
        Vector3 intersectedPoint,
        MaterialData materialData
    )
    {
        IntersectedObject = intersectedObject;
        IntersectedPoint = intersectedPoint;
        MaterialData = materialData;
    }
}