using System;
using System.Collections.Generic;

namespace RayTracing2._0
{
    public class Sphere: ISceneObject
    {
        public Vector3 Center;
        public double Radius;
        private readonly double _squareRadius;
        public VecColor Color { get; set; }
        public double ReflectCoef { get; private set; }
        public double SpecularCoef { get; private set; }

        public Sphere(Vector3 center, double radius, VecColor color, double reflectCoef, double specularCoef)
        {
            Center = center;
            Radius = radius;
            Color = color;
            _squareRadius = Radius * Radius;
            ReflectCoef = reflectCoef;
            SpecularCoef = specularCoef;
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint)
        {
            var normalPoint = crossPoint - Center;
            return normalPoint / normalPoint.Lenght;
        }

        public IEnumerable<double> FindIntersectedRayCoefficients(Ray ray)
        {
            var oc = ray.StartPoint - Center;

            var k1 = ray.DirectionDot;
            var k2 = 2 * Vector3.DotProduct(oc, ray.Direction);
            var k3 = Vector3.DotProduct(oc, oc) - _squareRadius;

            var discriminant = k2 * k2 - 4 * k1 * k3;
            if (discriminant < 0)
            {
                yield break;
            }

            yield return (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
            yield return (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
            
        }
    }
}