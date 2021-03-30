using System;

namespace RayTracing2._0
{
    public class SphereVector
    {
        public double Radius;
        public double Teta;
        public double Fi;

        public SphereVector(double radius, double teta, double fi)
        {
            Radius = radius;
            Teta = teta;
            Fi = fi;
        }

        public Vector3 ToCartesian()
        {
            return new Vector3(Radius * Math.Sin(Teta) * Math.Cos(Fi), Radius * Math.Sin(Teta) * Math.Sin(Fi),
                Radius * Math.Cos(Teta));
        }
    }
}