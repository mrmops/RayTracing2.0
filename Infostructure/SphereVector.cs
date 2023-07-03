using System;
using System.Numerics;

namespace RayTracing2._0.Infostructure;

public class SphereVector
{
    public float Radius;
    public float Teta;
    public float Fi;

    public SphereVector(float radius, float teta, float fi)
    {
        Radius = radius;
        Teta = teta;
        Fi = fi;
    }

    public Vector3 ToCartesian()
    {
        return new Vector3(
            (float)(Radius * Math.Sin(Teta) * Math.Cos(Fi)),
            (float)(Radius * Math.Sin(Teta) * Math.Sin(Fi)),
            (float)(Radius * Math.Cos(Teta))
        );
    }
}