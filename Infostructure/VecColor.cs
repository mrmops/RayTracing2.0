using System;
using System.Drawing;

namespace RayTracing2._0
{
    public class VecColor
    {
        private int R
        {
            get => _r;
            set => _r = (byte)Math.Min(255, Math.Max(0, value));
        }

        private int G
        {
            get => _g;
            set => _g = (byte)Math.Min(255, Math.Max(0, value));
        }

        private int B
        {
            get => _b;
            set => _b = (byte)Math.Min(255, Math.Max(0, value));
        }

        private byte _r;
        private byte _g;
        private byte _b;

        public VecColor(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static VecColor Empty => new VecColor(0, 0, 0);

        public static VecColor Full => new VecColor(255, 255, 255);

        public Color ToColor()
        {
            return Color.FromArgb(255, _r, _g, _b);
        }

        public static VecColor FromColor(Color color)
        {
            return new VecColor(color.R, color.G, color.B);
        }

        public static VecColor operator +(VecColor c1, VecColor c2)
        {
            return new VecColor(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        }

        public static VecColor operator -(VecColor c1, VecColor c2)
        {
            return new VecColor(c1.R - c2.R, c1.G - c2.G, c1.B - c2.B);
        }

        public static VecColor operator *(VecColor c1, VecColor c2)
        {
            return new VecColor(c1.G * c2.B - c1.B * c2.G, c1.B * c2.R - c1.R * c2.B, c1.R * c2.G - c1.G * c2.R);
        }

        public static VecColor operator *(VecColor c1, double coef)
        {
            return new VecColor((int)(c1.R * coef), (int)(c1.G * coef), (int)(c1.B * coef));
        }

        public static VecColor operator +(VecColor c1, double coef)
        {
            return new VecColor((int)(c1.R + coef), (int)(c1.G + coef), (int)(c1.B + coef));
        }

        public static VecColor operator /(VecColor c1, double coef)
        {
            return new VecColor((int)(c1.R / coef), (int)(c1.G / coef), (int)(c1.B / coef));
        }

        public static VecColor operator *(double coef, VecColor c1)
        {
            return c1 * coef;
        }

        public static VecColor operator +(double coef, VecColor c1)
        {
            return c1 + coef;
        }

        public static VecColor Intersection(VecColor c1, VecColor c2, double coef)
        {
            return new VecColor((int)(c1.R * coef + c2.R * (1 - coef)), (int)(c1.G * coef + c2.G * (1 - coef)), (int)(c1.B * coef + c2.B * (1 - coef)));
        }

        public static VecColor Intersection(VecColor c1, VecColor c2)
        {
            return new VecColor(GetMinRealColorValue(c1.R, c2.R), GetMinRealColorValue(c1.G, c2.G), GetMinRealColorValue(c1.B, c2.B));
        }


        private static int GetMinRealColorValue(int canal1, int canal2)
        {
            return Math.Max(0, Math.Min(canal1, canal2));
        }
    }
}