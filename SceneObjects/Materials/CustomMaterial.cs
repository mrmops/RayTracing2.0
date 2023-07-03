using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects.Materials;

public class CustomMaterial : IMaterial
{
    public VecColor Color { get; }
    public double ReflectCoef { get; }
    public double SpecularCoef { get; }
    public double RefractiveCoef { get; }
    public double AbsorptionCoefficient { get; }

    public CustomMaterial(VecColor color, double reflectCoef, double specularCoef, double refractiveCoef, double absorptionCoefficient)
    {
        Color = color;
        ReflectCoef = reflectCoef;
        SpecularCoef = specularCoef;
        RefractiveCoef = refractiveCoef;
        AbsorptionCoefficient = absorptionCoefficient;
    }
}