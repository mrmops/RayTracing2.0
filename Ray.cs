namespace RayTracing2._0
{
    public class Ray
    {
        public Vector3 StartPoint { get; private set; }
        public Vector3 TargetPoint { get; private set; }

        public Ray(Vector3 startPoint, Vector3 targetPoint)
        {
            StartPoint = startPoint;
            TargetPoint = targetPoint;
        }


        public Vector3 GetPointFromCoefficient(double coefficient)
        {
            return StartPoint + (coefficient * TargetPoint);
        }
    }
}