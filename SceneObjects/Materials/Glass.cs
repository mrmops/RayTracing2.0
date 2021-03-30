namespace RayTracing2._0.Material
{
    public class Glass: IMaterial
    {
        public VecColor Color { get;  }
        public double ReflectCoef => 0;
        public double SpecularCoef { get; }
        public double RefractiveCoef => 1.57;
        public double AbsorptionCoefficient { get; }

        public Glass(VecColor color, double specularCoef, double absorptionCoefficient)
        {
            Color = color;
            SpecularCoef = specularCoef;
            AbsorptionCoefficient = absorptionCoefficient;
        }
    }
}