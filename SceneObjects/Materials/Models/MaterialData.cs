using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.SceneObjects.Materials.Models;

public record struct MaterialData(
    VecColor Color,
    Vector3 Normal,
    MaterialParameters Parameters
) {
    public static MaterialData FromMaterial(IMaterial material, Vector3 normal, VecColor? color = null) {
        return new MaterialData(
            color ?? material.Color,
            normal,
            new MaterialParameters(
                material.ReflectCoef,
                material.SpecularCoef,
                material.RefractiveCoef,
                material.AbsorptionCoefficient
            )
        );
    }
}
