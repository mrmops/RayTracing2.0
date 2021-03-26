using System.Collections.Generic;

namespace RayTracing2._0
{
    public interface ISceneObject
    {
        IEnumerable<double> FindIntersectsRay(Ray ray);
        Vector3 GetNormalUnitVector(Vector3 crossPoint);
        VecColor Color { get; }

        double ReflectCoef { get; }
        
        double SpecularCoef { get; }
    }
}