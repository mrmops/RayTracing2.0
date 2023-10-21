namespace RayTracing2._0.SceneObjects.Materials.Models;

public record MaterialParameters
(
    double ReflectCoefficient,
    double SpecularCoefficient,
    double RefractiveCoefficient,
    double AbsorptionCoefficient
);