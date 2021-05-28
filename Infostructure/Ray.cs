namespace RayTracing2._0
{
    public class Ray
    {
        public Vector3 StartPoint { get; }
        public Vector3 Direction { get; }
        
        
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

        public override int GetHashCode()
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + StartPoint.GetHashCode();
            hash = hash * 23 + Direction.GetHashCode();
            return hash;
        }
    }
}