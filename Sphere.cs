using System;
using System.Collections.Generic;

namespace RayTracing2._0
{
    public class Sphere: ISceneObject
    {
        public Vector3 Center;
        public double Radius;
        public VecColor Color { get; set; }

        public Sphere(Vector3 center, double radius, VecColor color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint)
        {
            var normalPoint = crossPoint - Center;
            return normalPoint / normalPoint.Lenght;
        }

        public IEnumerable<double> FindIntersectsRay(Ray ray)
        {
            var oc = ray.StartPoint - Center;

            var k1 = Vector3.DotProduct(ray.TargetPoint, ray.TargetPoint);
            var k2 = 2 * Vector3.DotProduct(oc, ray.TargetPoint);
            var k3 = Vector3.DotProduct(oc, oc) - Radius * Radius;

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