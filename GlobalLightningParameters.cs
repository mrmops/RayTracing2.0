using System.Drawing;

namespace RayTracing2._0
{
    public static class GlobalLightningParameters
    {
        public static double BackgroundIlluminationRatio = 10;
        public static double BackgroundIntensityRatio = 0.01;
        public static double DiffusionIlluminationCoefficient = 1;
        public static double ReflectedBeamRatio = 0.75;
        public static double IlluminanceFactorCoefficient = 0.75;
        public static double FongCoefficient = 5;

        public static VecColor BackgroundLight => BackgroundIlluminationRatio * BackgroundIntensityRatio * VecColor.Full;
    }
}