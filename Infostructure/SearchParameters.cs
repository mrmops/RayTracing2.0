namespace RayTracing2._0
{
    public class SearchParameters
    {
        public Ray Ray { get; private set; }

        public double Min{ get; private set; }
        public double Max{ get; private set; }

        public SearchParameters(Ray ray, double tMin, double tMax)
        {
            Ray = ray;
            Min = tMin;
            Max = tMax;
        }
    }
}