using System.Numerics;

namespace RayTracing2._0.SceneObjects
{
    public class Triangle
    {
        private Vector3 _e1;
        private Vector3 _e2;
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
            _e1 = B - A;
            _e2 = C - A;
        }

        public bool FindIntersectedRayCoefficients(Ray ray, out float t)
        {
            t = 0;
            var pVec = Vector3.Cross(ray.Direction, _e2);
            var det = Vector3.Dot(_e1, pVec);
            if (det < 1e-8 && det > -1e-8)
                return false;

            var invDet = 1 / det;
            var tVec = ray.StartPoint - A;
            var u = Vector3.Dot(tVec, pVec) * invDet;
            if (u < 0 || u > 1)
                return false;

            var qVec = Vector3.Cross(tVec, _e1);
            var v = Vector3.Dot(ray.Direction, qVec) * invDet;
            if (v < 0 || u + v > 1)
                return false;

            t = Vector3.Dot(_e2, qVec) * invDet;
            return true;
        }
    }
}