using System.Drawing;
using System.Numerics;

namespace RayTracing2._0
{
    public class TraceRayResult
    {
        public ISceneObject IntersectedObject;
        public Vector3 IntersectedPoint;
        public VecColor ResultColor;

        public TraceRayResult(ISceneObject intersectedObject, Vector3 intersectedPoint, VecColor resultColor)
        {
            IntersectedObject = intersectedObject;
            IntersectedPoint = intersectedPoint;
            ResultColor = resultColor;
        }
    }
}