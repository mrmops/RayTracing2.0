
namespace RayTracing2._0
{
    public class RayTraceResult
    {
        public bool Success { get; private set; }
        public ISceneObject CrossObject{ get; private set; }
        public Vector3 CrossPoint{ get; private set; }
        public VecColor VecColor => ReflectionLight.ToResultColor(CrossObject);
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
}