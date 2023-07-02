using System.Collections.Generic;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0.SceneObjects
{
    public interface ISceneObject
    {
        IEnumerable<(float, Vector3)> FindIntersectedRayCoefficients(Ray ray);

        IMaterial Material { get; }
        Vector3 Location { get; }
    }
}