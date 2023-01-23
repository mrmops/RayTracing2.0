using System.Numerics;

namespace RayTracing2._0
{
    public class PointLight : Light
    {
        public Vector3 Location;

        public PointLight(Vector3 location, VecColor color, double intensity) : base(color, intensity)
        {
            Location = location;
            Color = color;
            Intensity = intensity;
        }

        public override SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint)
        {
            return new SearchParameters(new Ray(crossPoint, Location - crossPoint), 0.001f, 1);
        }

        public override Vector3 GetDirection(Vector3 point)
        {
            return Location - point;
        }
    }
}