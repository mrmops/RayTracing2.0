using System.Collections.Generic;
using RayTracing2._0.Material;

namespace RayTracing2._0
{
    public interface ISceneObject
    {
        IEnumerable<double> FindIntersectedRayCoefficients(Ray ray);
        
        Vector3 GetNormalUnitVector(Vector3 crossPoint);

        IMaterial Material { get; }

    }
}