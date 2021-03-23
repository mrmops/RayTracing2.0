
namespace RayTracing2._0
{
    public class RayTraceResult
    {
        public bool Success { get; private set; }
        public ISceneObject CrossObject{ get; private set; }
        public Vector3 CrossPoint{ get; private set; }
        public VecColor VecColor => VecColor.Intersection(ReflectionLight.ToResultColor(), CrossObject.Color);
        public ReflectionLight ReflectionLight { get; private set; }

        public RayTraceResult(bool success, ISceneObject crossObject = null, Vector3 crossPoint = null, ReflectionLight reflectionLight = null)
        {
            Success = success;
            CrossObject = crossObject;
            CrossPoint = crossPoint;
            ReflectionLight = reflectionLight;
        }

        public static RayTraceResult Fail => new RayTraceResult(false);
    }

    public class ReflectionLight
    {
        public VecColor DiffusedColor{ get; private set; }
        public VecColor ReflectColor{ get; private set; }
        public VecColor PrimaryIllumination { get; private set; }

        public ReflectionLight(VecColor diffusedColor, VecColor reflectColor, VecColor primaryIllumination)
        {
            DiffusedColor = diffusedColor;
            ReflectColor = reflectColor;
            PrimaryIllumination = primaryIllumination;
        }

        public VecColor ToResultColor()
        {
            return GlobalLightningParameters.BackgroundLight
                   + DiffusedColor
                   + ReflectColor
                   + PrimaryIllumination;
        }
    }
}