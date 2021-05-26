using System.Drawing;

namespace RayTracing2._0
{
    public class TraceRayResult
    {
        public readonly bool Success;
        public ISceneObject IntersectedObject;
        public Vector3 IntersectedPoint;
        public VecColor ResultColor;

        public TraceRayResult(bool success, ISceneObject intersectedObject, Vector3 intersectedPoint, VecColor resultColor)
        {
            Success = success;
            IntersectedObject = intersectedObject;
            IntersectedPoint = intersectedPoint;
            ResultColor = resultColor;
        }

        public static TraceRayResult Fail() => new TraceRayResult(false, null, null, VecColor.FromColor(Color.SkyBlue));
    }
}