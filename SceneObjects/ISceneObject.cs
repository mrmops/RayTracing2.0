using RayTracing2._0.Material;
using System.Collections.Generic;
using System.Numerics;

namespace RayTracing2._0
{
    public interface ISceneObject
    {
        IEnumerable<(float, Vector3)> FindIntersectedRayCoefficients(Ray ray);

        IMaterial Material { get; }
        Vector3 Location { get; }
    }
}