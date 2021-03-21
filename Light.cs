namespace RayTracing2._0
{
    public class Light
    {
        public Vector Location;
        public VecColor Color;
        public double Intensity;

        public Light(Vector location, VecColor color, double intensity)
        {
            Location = location;
            Color = color;
            Intensity = intensity;
        }
    }
}