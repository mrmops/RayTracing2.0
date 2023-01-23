using System.Collections.Generic;
using RayTracing2._0.Material;

namespace RayTracing2._0
{
    public interface ISceneObject
    {
        IEnumerable<(double, Vector3)> FindIntersectedRayCoefficients(Ray ray);
        
        IMaterial Material { get; }
        Vector3 Location { get; }
    }
}