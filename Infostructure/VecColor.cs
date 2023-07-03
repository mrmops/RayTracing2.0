using System;
using System.Drawing;

namespace RayTracing2._0.Infostructure;

public class VecColor
{
    private int R
    {
        get => _r;
        init => _r = (byte)Math.Min(255, Math.Max(0, value));
    }

    private int G
    {
        get => _g;
        init => _g = (byte)Math.Min(255, Math.Max(0, value));
    }

    private int B
    {
        get => _b;
        init => _b = (byte)Math.Min(255, Math.Max(0, value));
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

    public static VecColor Empty => new(0, 0, 0);

    public static VecColor Full => new(255, 255, 255);

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

    public static VecColor Intersection(VecColor c1, VecColor c2)
    {
        return new VecColor(_getMinRealColorValue(c1.R, c2.R), _getMinRealColorValue(c1.G, c2.G), _getMinRealColorValue(c1.B, c2.B));
    }


    private static int _getMinRealColorValue(int canal1, int canal2)
    {
        return Math.Max(0, Math.Min(canal1, canal2));
    }
}