using System;
using System.Collections.Generic;
using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects.Objects.Spheres;

public class SphereBase
{
    public readonly Vector3 Center;
    public readonly double Radius;
    private readonly double _squareRadius;

    public SphereBase(Vector3 center, double radius)
    {
        Center = center;
        Radius = radius;
        _squareRadius = radius * radius;
    }

    public Vector3 GetNormalUnitVector(Vector3 crossPoint)
    {
        var normalPoint = crossPoint - Center;
        return Vector3.Normalize(normalPoint);
    }

    public IEnumerable<float> GetIntersectedRayCoefficients(Ray ray)
    {
        var oc = ray.StartPoint - Center;

        var a = ray.DirectionDot;
        var b = 2 * Vector3.Dot(oc, ray.Direction);
        var c = Vector3.Dot(oc, oc) - _squareRadius;

        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            yield break;
        }

        var doubleA = 2 * a;
        var minusB = -b;
        var rootOfDiscriminant = Math.Sqrt(discriminant);

        yield return (float)(minusB + rootOfDiscriminant) / doubleA;

        yield return ((float)(minusB - rootOfDiscriminant) / doubleA);
    }
}