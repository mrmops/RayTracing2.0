namespace RayTracing2._0
{
    public class PointLight: Light
    {
        public Vector3 Location;

        public PointLight(Vector3 location, VecColor color, double intensity)  : base(color, intensity)
        {
            Location = location;
            Color = color;
            Intensity = intensity;
        }

        public override SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint)
        {
            return new SearchParameters(new Ray(crossPoint, Location - crossPoint), 0.001, 1);
        }

        public override Vector3 GetDirection(Vector3 crossPoint)
        {
            return Location - crossPoint;
        }
    }

    public abstract class Light: ILight
    {
        public VecColor Color;
        public double Intensity;

        public Light(VecColor color, double intensity)
        {
            Color = color;
            Intensity = intensity;
        }

        public abstract SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint);
        public abstract Vector3 GetDirection(Vector3 crossPoint);
    }

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
            return new SearchParameters(new Ray(crossPoint, crossPoint + Direction), 0.001,
                double.PositiveInfinity);
        }

        public override Vector3 GetDirection(Vector3 crossPoint)
        {
            return Direction;
        }
    }
}