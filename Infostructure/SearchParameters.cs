namespace RayTracing2._0.Infostructure
{
    public class SearchParameters
    {
        public Ray Ray { get; }

        public float MinRayCoefficient { get; }
        public float MaxRayCoefficient { get; }

        public SearchParameters(Ray ray, float minRayCoefficient = 0.001f, float maxRayCoefficient = float.PositiveInfinity)
        {
            Ray = ray;
            MinRayCoefficient = minRayCoefficient;
            MaxRayCoefficient = maxRayCoefficient;
        }
    }
}