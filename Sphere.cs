namespace RayTracing2._0
{
    public class Sphere
    {
        public Vector Center;
        public double Radius;
        public VecColor Color;

        public Sphere(Vector center, double radius, VecColor color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }

        public Vector GetNormalUnitVector(Vector crossPoint)
        {
            var normalPoint = crossPoint - Center;
            return normalPoint / normalPoint.Lenght;
        }
    }
}