using System.Collections.Generic;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects;

public interface ISceneObject
{
    IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray);
}