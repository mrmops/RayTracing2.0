using System.Collections.Generic;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0.SceneObjects;

public interface ISceneObject
{
    IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray);
}