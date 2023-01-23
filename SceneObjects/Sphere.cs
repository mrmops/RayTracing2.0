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

            var k1 = ray.DirectionDot;
            var k2 = 2 * Vector3.Dot(oc, ray.Direction);
            var k3 = Vector3.Dot(oc, oc) - _squareRadius;

            var discriminant = k2 * k2 - 4 * k1 * k3;
            if (discriminant < 0)
            {
                yield break;
            }

            var k12 = 2 * k1;
            var minusK = -k2;

            var findIntersectedRayCoefficients = (minusK + (float)Math.Sqrt(discriminant)) / k12;
            yield return (findIntersectedRayCoefficients, GetNormalUnitVector(ray.GetPointFromCoefficient(findIntersectedRayCoefficients)));

            var intersectedRayCoefficients = (minusK - (float) Math.Sqrt(discriminant)) / k12;
            yield return (intersectedRayCoefficients, GetNormalUnitVector(ray.GetPointFromCoefficient(intersectedRayCoefficients)));

        }
    }
}