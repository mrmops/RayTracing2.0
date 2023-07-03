using System.Drawing;
using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0;

public class TraceRayResult
{
    public readonly IntersectionResult IntersectionResult;
    public readonly VecColor ResultColor;

    public TraceRayResult(IntersectionResult intersectionResult, VecColor resultColor) {
        IntersectionResult = intersectionResult;
        ResultColor = resultColor;
    }
}