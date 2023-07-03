using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects.Materials;

public record MaterialCoefficientData(
    VecColor Color,
    Vector3 Normal,
    double ReflectCoefficient,
    double SpecularCoefficient,
    double RefractiveCoefficient,
    double AbsorptionCoefficient
)
{
    public static MaterialCoefficientData FromMaterial(IMaterial material, Vector3 normal)
    {
        return new MaterialCoefficientData(material.Color, normal,
            material.ReflectCoef,
            material.SpecularCoef,
            material.RefractiveCoef,
            material.AbsorptionCoefficient
        );
    }
}