using System.Numerics;
using RayTracing2._0.SceneObjects;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0;

public struct IntersectionResult
{
    public ISceneObject IntersectedObject;
    public Vector3 IntersectedPoint;
    public MaterialCoefficientData MaterialCoefficientData;

    public IntersectionResult(
        ISceneObject intersectedObject,
        Vector3 intersectedPoint,
        MaterialCoefficientData materialCoefficientData
    )
    {
        IntersectedObject = intersectedObject;
        IntersectedPoint = intersectedPoint;
        MaterialCoefficientData = materialCoefficientData;
    }
}