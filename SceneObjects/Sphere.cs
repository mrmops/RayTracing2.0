using System;
using System.Collections.Generic;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0.SceneObjects
{
    public class Sphere : ISceneObject
    {
        private readonly Vector3 _center;
        public readonly double Radius;
        private readonly double _squareRadius;
        public IMaterial Material { get; }
        public Vector3 Location => _center;

        public Sphere(Vector3 center, double radius, IMaterial material)
        {
            _center = center;
            Radius = radius;
            Material = material;
            _squareRadius = Radius * Radius;
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint)
        {
            var normalPoint = crossPoint - _center;
            return Vector3.Normalize(normalPoint);
        }

        public IEnumerable<(float, Vector3)> FindIntersectedRayCoefficients(Ray ray)
        {
            var oc = ray.StartPoint - _center;

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

            yield return _findIntersectionResult((float)(minusB + rootOfDiscriminant) / doubleA, ray);

            yield return _findIntersectionResult((float)(minusB - rootOfDiscriminant) / doubleA, ray);
        }

        private (float, Vector3) _findIntersectionResult(float rayIntersectionCoef, Ray ray)
        {
            return (rayIntersectionCoef, GetNormalUnitVector(ray.GetPointFromCoefficient(rayIntersectionCoef)));
        }
    }
}