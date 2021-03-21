using System;

namespace RayTracing2._0
{
    public class Point3D
    {
        public double X;
        public double Y;
        public double Z;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Lenght()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public static Point3D operator -(Point3D point1, Point3D point2)
        {
            return new Point3D(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }
        
        public static Point3D operator +(Point3D point1, Point3D point2)
        {
            return new Point3D(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }
        
        public static double Distance(Point3D point1, Point3D point2)
        {
            //var cosBetween = CosBetween(point1, point2);
            return point1.X * point2.X + point1.Y * point2.Y + point1.Z * point2.Z;
        }
        
        public static double CosBetween(Point3D point1, Point3D point2)
        {
            return (point1.X * point2.X + point1.Y * point2.Y + point1.Z * point2.Z) /
                   (Math.Sqrt(point1.X * point1.X + point1.Y * point1.Y + point1.Z * point1.Z) *
                    Math.Sqrt(point2.X * point2.X + point2.Y * point2.Y + point2.Z * point2.Z));
        }
    }
}