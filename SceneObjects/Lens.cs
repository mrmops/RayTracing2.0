using System;
using System.Collections.Generic;
using System.Linq;
using RayTracing2._0.Material;

namespace RayTracing2._0.SceneObjects
{
    public class Lens : ISceneObject
    {
        private Sphere _sphere1;
        private Sphere _sphere2;

        public Lens(IMaterial material, Vector3 location, double radius)
        {
            Material = material;
            Location = location;
            _sphere1 = new Sphere(location, radius, material);
            var d = radius / 2;
            _sphere2 = new Sphere(location + new Vector3(0, 0, d), radius + radius / 3, material);
        }

        public IEnumerable<(double, Vector3)> FindIntersectedRayCoefficients(Ray ray)
        {
            var int1 = _sphere1.FindIntersectedRayCoefficients(ray);
            var int2 = _sphere2.FindIntersectedRayCoefficients(ray);
            foreach (var valueTuple in int1.Where(
                x => (ray.GetPointFromCoefficient(x.Item1) - _sphere2.Location).Lenght > _sphere2.Radius
            ))
            {
                yield return valueTuple;
            }
            
            foreach (var valueTuple in int2)
            {
                var pointFromCoefficient = ray.GetPointFromCoefficient(valueTuple.Item1);
                if((pointFromCoefficient - _sphere1.Location).Lenght < _sphere1.Radius)
                {
                    yield return (valueTuple.Item1, -1 * _sphere2.GetNormalUnitVector(pointFromCoefficient));
                }
            }
        }

        public Vector3 GetNormalUnitVector(Vector3 crossPoint)
        {
            if (Math.Abs((crossPoint - _sphere2.Location).Lenght - _sphere2.Radius) < 1e-3)
            {
                return -1 * _sphere2.GetNormalUnitVector(crossPoint);
            }

            return _sphere1.GetNormalUnitVector(crossPoint);
        }

        public IMaterial Material { get; }
        public Vector3 Location { get; }
    }
}