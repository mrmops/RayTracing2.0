namespace RayTracing2._0
{
    public class ReflectionLight
    {
        public VecColor DiffusedColor { get; private set; }
        public VecColor ReflectColor { get; private set; }
        public VecColor PrimaryIllumination { get; private set; }

        public ReflectionLight(VecColor diffusedColor, VecColor reflectColor, VecColor primaryIllumination)
        {
            DiffusedColor = diffusedColor;
            ReflectColor = reflectColor;
            PrimaryIllumination = primaryIllumination;
        }

        public VecColor ToResultColor(ISceneObject sceneObject)
        {
            var reflectCoef = sceneObject.Material.ReflectCoef;
            if (reflectCoef < 0.99999)
            {
                var normalColor = VecColor.Intersection(
                    (GlobalLightningParameters.BackgroundLight + DiffusedColor),
                    sceneObject.Material.Color);
                return normalColor * (1 - reflectCoef) + ReflectColor * reflectCoef + PrimaryIllumination;
            }

            return ReflectColor + PrimaryIllumination;
        }
    }
}