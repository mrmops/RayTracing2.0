using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.Lights;

public abstract class Light : ILight
{
    public VecColor Color { get; protected set; }
    public double Intensity;

    public Light(VecColor color, double intensity)
    {
        Color = color;
        Intensity = intensity;
    }

    public abstract SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint);
    public abstract Vector3 GetDirection(Vector3 point);
}