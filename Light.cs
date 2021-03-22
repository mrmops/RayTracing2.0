namespace RayTracing2._0
{
    public class Light
    {
        public Vector3 Location;
        public VecColor Color;
        public double Intensity;

        public Light(Vector3 location, VecColor color, double intensity)
        {
            Location = location;
            Color = color;
            Intensity = intensity;
        }
    }
}