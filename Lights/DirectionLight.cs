using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.Lights
{
    public class DirectionLight : Light
    {
        public Vector3 Direction;
        public DirectionLight(Vector3 direction, VecColor color, double intensity) : base(color, intensity)
        {
            Direction = direction;
            Color = color;
            Intensity = intensity;
        }



        public override SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint)
        {
            return new SearchParameters(new Ray(crossPoint, Direction));
        }

        public override Vector3 GetDirection(Vector3 point)
        {
            return Direction;
        }
    }
}