namespace RayTracing2._0
{
    public class Ray
    {
        public Vector3 StartPoint { get; private set; }
        public Vector3 Direction { get; private set; }
        
        public double DirectionDot { get; private set; }

        public Ray(Vector3 startPoint, Vector3 direction)
        {
            StartPoint = startPoint;
            Direction = direction;
            DirectionDot = Vector3.DotProduct(Direction, Direction);
        }


        public Vector3 GetPointFromCoefficient(double coefficient)
        {
            return StartPoint + (coefficient * (Direction));
        }
    }
}