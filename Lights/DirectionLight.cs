using System.Collections.Generic;

namespace RayTracing2._0
{
    public class DirectionLight: Light
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
            return new SearchParameters(new Ray(crossPoint, Direction), 0.001,
                double.PositiveInfinity);
        }

        public override Vector3 GetDirection(Vector3 crossPoint)
        {
            return Direction;
        }
    }
}