namespace RayTracing2._0.SceneObjects
{
    public class Triangle
    {
        public Vector3 A { get; private set; }
        public Vector3 B { get; private set; }
        public Vector3 C { get; private set; }
        
        public Vector3 Normal { get; private set; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            A = a;
            B = b;
            C = c;
            Normal = normal;
        }

        public bool FindIntersectedRayCoefficients(Ray ray, out double t)
        {
            t = 0;
            var E1 = B - A;
            var E2 = C - A;
            var pVec = Vector3.CombineVector(ray.Direction, E2);
            var det = Vector3.DotProduct(E1, pVec);
            if (det < 1e-8 && det > -1e-8)
                return false;

            var invDet = 1 / det;
            var tVec = ray.StartPoint - A;
            var u = Vector3.DotProduct(tVec, pVec) * invDet;
            if (u < 0 || u > 1)
                return false;

            var qVec = Vector3.CombineVector(tVec, E1);
            var v = Vector3.DotProduct(ray.Direction, qVec) * invDet;
            if (v < 0 || u + v > 1)
                return false;

            t =  Vector3.DotProduct(E2, qVec) * invDet;
            return true;
        }
    }
}