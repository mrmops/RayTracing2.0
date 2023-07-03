using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects.Materials;

public interface IMaterial
{
    VecColor Color { get; }

    double ReflectCoef { get; }

    double SpecularCoef { get; }

    double RefractiveCoef { get; }

    double AbsorptionCoefficient { get; }
}