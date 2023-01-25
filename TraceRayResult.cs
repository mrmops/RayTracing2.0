using System.Drawing;
using System.Numerics;

namespace RayTracing2._0
{
    public class TraceRayResult
    {
        public readonly IntersectionResult? IntersectionResult;
        public readonly VecColor ResultColor;

        public bool isSuccess => IntersectionResult.HasValue;

        public TraceRayResult(IntersectionResult? intersectionResult, VecColor resultColor) {
            IntersectionResult = intersectionResult;
            ResultColor = resultColor;
        }

        public static TraceRayResult Fail() {
            return new TraceRayResult(null, VecColor.FromColor(Color.SkyBlue));
        }
    }
}