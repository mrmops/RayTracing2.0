namespace RayTracing2._0.Material
{
    public interface IMaterial
    {
        VecColor Color { get; }

        double ReflectCoef { get; }
        
        double SpecularCoef { get; }
        
        double RefractiveCoef { get; }
        
        double AbsorptionCoefficient { get; }
    }
}