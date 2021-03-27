using System;

namespace RayTracing2._0
{
    public class Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Lenght => Math.Sqrt(X * X + Y * Y + Z * Z);
        
        public Matrix ToMatrix()
        {
            return new Matrix(new double[4, 1]
            {
                {X},
                {Y},
                {Z},
                {1}
            });
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public static Vector3 operator -(Vector3 point1, Vector3 point2)
        {
            return new Vector3(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }
        
        public static Vector3 operator +(Vector3 point1, Vector3 point2)
        {
            return new Vector3(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }
        
        public static Vector3 operator *(Vector3 point1, double n)
        {
            return new Vector3(point1.X * n, point1.Y * n, point1.Z * n);
        }
        
        public static Vector3 operator *(double n, Vector3 point1)
        {
            return point1 * n;
        }
        
        public static Vector3 CombineVector(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }
        
        public static Vector3 operator /(Vector3 point1, double n)
        {
            return new Vector3(point1.X / n, point1.Y / n, point1.Z / n);
        }
        
        public static double DotProduct(Vector3 point1, Vector3 point2)
        {
            //var cosBetween = CosBetween(point1, point2);
            return point1.X * point2.X + point1.Y * point2.Y + point1.Z * point2.Z;
        }
        
        public static double CosBetween(Vector3 point1, Vector3 point2)
        {
            return DotProduct(point1, point2) / (point1.Lenght * point2.Lenght);
        }
    }
}