
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Objects.Spheres;

namespace RayTracing2._0.SceneObjects.Objects;

public class Lens : ISceneObject, IRenderObject
{
    private readonly Sphere _sphere1;
    private readonly Sphere _sphere2;
    private IMaterial Material { get; }

    public Lens(IMaterial material, Vector3 location, float radius)
    {
        Material = material;
        _sphere1 = new Spheres.Sphere(location, radius, material);
        var d = radius / 2;
        _sphere2 = new Spheres.Sphere(location + new Vector3(0, 0, d), radius + radius / 3, material);
    }

    public IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray)
    {
        var int1 = _sphere1.FindIntersectedRayCoefficients(ray);
        var int2 = _sphere2.FindIntersectedRayCoefficients(ray);
        foreach (var valueTuple in int1.Where(
                     x => Vector3.Distance(ray.GetPointFromCoefficient(x.Item1), _sphere2.Center) > _sphere2.Radius
                 ))
        {
            yield return valueTuple;
        }

        foreach (var valueTuple in int2)
        {
            var pointFromCoefficient = ray.GetPointFromCoefficient(valueTuple.Item1);
            if (Vector3.Distance(pointFromCoefficient, _sphere1.Center) < _sphere1.Radius)
            {
                yield return (valueTuple.Item1, this);
            }
        }
    }

    public MaterialCoefficientData GetMaterialByIntersection(Vector3 intersectionPoint)
    {
        return MaterialCoefficientData.FromMaterial(Material, _getNormalUnitVector(intersectionPoint));
    }

    private Vector3 _getNormalUnitVector(Vector3 crossPoint)
    {
        if (Math.Abs(Vector3.Distance(crossPoint, _sphere2.Center) - _sphere2.Radius) < 1e-3)
        {
            return -1 * _sphere2.GetNormalUnitVector(crossPoint);
        }

        return _sphere1.GetNormalUnitVector(crossPoint);
    }
}