namespace RayTracing2._0
{
    public class SearchParameters
    {
        public Ray Ray { get; private set; }

        public float MinRayCoefficient { get; private set; }
        public float MaxRayCoefficient { get; private set; }

        public SearchParameters(Ray ray, float minRayCoefficient = 0.001f, float maxRayCoefficient = float.PositiveInfinity)
        {
            Ray = ray;
            MinRayCoefficient = minRayCoefficient;
            MaxRayCoefficient = maxRayCoefficient;
        }
    }
}