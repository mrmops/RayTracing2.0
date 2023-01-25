namespace RayTracing2._0.Material
{
    public class EmptyMaterial : IMaterial
    {
        public VecColor Color => VecColor.Empty;
        public double ReflectCoef => 0;
        public double SpecularCoef => 0;
        public double RefractiveCoef => 0;
        public double AbsorptionCoefficient => 0;
    }
}