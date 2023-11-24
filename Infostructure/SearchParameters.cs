namespace RayTracing2._0.Infostructure;

public struct SearchParameters
{
    public Ray Ray { get; }

    public float MinRayLengthCoefficient { get; }
    public float MaxRayCoefficient { get; }

    public SearchParameters(Ray ray, float minRayLengthCoefficient = 0.001f, float maxRayCoefficient = float.PositiveInfinity)
    {
        Ray = ray;
        MinRayLengthCoefficient = minRayLengthCoefficient;
        MaxRayCoefficient = maxRayCoefficient;
    }
}