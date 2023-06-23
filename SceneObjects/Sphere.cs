using RayTracing2._0.Material;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace RayTracing2._0
{
    public class Sphere : ISceneObject
    {
        public Vector3 Center;
        public double Radius;
        private readonly double _squareRadius;
        public IMaterial Material { get; }
        public Vector3 Location => Center;

        public Sphere(Vector3 center, double radius, IMaterial material)
        {
            Center = center;
            Radius = radius;
            Material = material;
            _squareRadius = Radius * Radius;
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint)
        {
            var normalPoint = crossPoint - Center;
            return Vector3.Normalize(normalPoint);
        }

        public IEnumerable<(float, Vector3)> FindIntersectedRayCoefficients(Ray ray)
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
            var rootOfDiscriminant = (float)Math.Sqrt(discriminant);

            var firstIntersectedRayCoefficient = (minusB + rootOfDiscriminant) / doubleA;
            yield return (firstIntersectedRayCoefficient, GetNormalUnitVector(ray.GetPointFromCoefficient(firstIntersectedRayCoefficient)));

            var secondIntersectedRayCoefficient = (minusB - rootOfDiscriminant) / doubleA;
            yield return (secondIntersectedRayCoefficient, GetNormalUnitVector(ray.GetPointFromCoefficient(secondIntersectedRayCoefficient)));

        }
    }
}