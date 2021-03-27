using System;

namespace RayTracing2._0
{
    public class SearchParameters
    {
        public Ray Ray { get; private set; }

        public double MinRayCoefficient{ get; private set; }
        public double MaxRayCoefficient{ get; private set; }

        public SearchParameters(Ray ray, double minRayCoefficient = 0.001, double maxRayCoefficient = double.PositiveInfinity)
        {
            Ray = ray;
            MinRayCoefficient = minRayCoefficient;
            MaxRayCoefficient = maxRayCoefficient;
        }
    }
}