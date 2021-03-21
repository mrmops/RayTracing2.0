using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracing2._0
{
    public partial class Form1 : Form
    {

        private Size canvasSize;
        private Timer _timer = new Timer() {Interval = 1};
        private Size _viewCanvas = new Size(1, 1);
        private List<Sphere> _spheres = new List<Sphere>()
        {
            new Sphere(new Point3D(1, 1, 3), 0.5, Color.Brown)
        };

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            // WindowState = FormWindowState.Maximized;
            canvasSize = new Size(pictureBox.Width, pictureBox.Height);
            pictureBox.SizeChanged += (sender, args) =>
            {
                canvasSize = new Size(pictureBox.Width, pictureBox.Height);
            };
            _timer.Tick += async (sender, args) =>
            {
                var image = await UpdateCanvasAsync();
                pictureBox.Image = image;
            };
            
            _timer.Start();
        }
        
        private Task<Bitmap> UpdateCanvasAsync()
        {
            var task = new Task<Bitmap>(() =>
            {
                var camera = new Point3D(0.0, 0.0, 0.0);
                var canvas = new Bitmap(this.canvasSize.Width, this.canvasSize.Height);
                for (int x = 0; x < canvas.Width; x++)
                {
                    Parallel.For(0, canvas.Height, y =>
                    {
                        var viewPortPosition = CanvasToViewport(x, y);
                        var color = TraceRay(camera, viewPortPosition, 1, Double.PositiveInfinity);
                        lock (canvas)
                        {
                            canvas.SetPixel(x, y, color);
                        }
                    });
                    // for (int y = 0; y < canvas.Height; y++)
                    // {
                    //     var viewPortPosition = CanvasToViewport(x, y);
                    //     var color = TraceRay(camera, viewPortPosition, 1, Double.PositiveInfinity);
                    //     canvas.SetPixel(x, y, color);
                    // }
                }
                return canvas;
            });
            task.Start();
            return task;
        }

        // private void UpdateCanvas()
        // {
        //     var camera = new Point3D(0.0, 0.0, 0.0);
        //     for (int x = 0; x < _canvas.Width; x++)
        //     {
        //         for (int y = 0; y < _canvas.Height; y++)
        //         {
        //             var viewPortPosition = CanvasToViewport(x, y);
        //             var color = TraceRay(camera, viewPortPosition, 1, Double.PositiveInfinity);
        //             _canvas.SetPixel(x, y, color);
        //         }
        //     }
        //     pictureBox.Image = _canvas;
        // }
        
        private Point3D CanvasToViewport(int x, int y)
        {
            return new Point3D(
                x * (double)_viewCanvas.Width / canvasSize.Width,
                y * (double)_viewCanvas.Height / canvasSize.Height,
                _lenghtToNearBorder
            );
        }

        private Color TraceRay(Point3D camera, Point3D viewPortPosition, int tMin, double tMax)
        {
            Sphere closestSphere = null;
            foreach (var sphere in _spheres)
            {
                (var t1, var t2) = IntersectRaySphere(camera, viewPortPosition, sphere);
                if (t1 > tMin && t2 < tMax && t1 < Double.PositiveInfinity)
                {
                    tMax = t1;
                    closestSphere = sphere;
                }
                if (t2 > tMin && t2 < tMax && t2 < Double.PositiveInfinity)
                {
                    tMax = t2;
                    closestSphere = sphere;
                }
            }

            if (closestSphere == null)
                return BackColor;
            return closestSphere.Color;
        }

        private (double, double) IntersectRaySphere(Point3D camera, Point3D viewPortPosition, Sphere sphere)
        {
            var sphereCenter = sphere.Center;
            var r = sphere.Radius;
            var oc = camera - sphereCenter;

            var k1 = Point3D.Distance(viewPortPosition, viewPortPosition);
            var k2 = 2 * Point3D.Distance(oc, viewPortPosition);
            var k3 = Point3D.Distance(oc, oc) - r * r;

            var discriminant = k2 * k2 - 4 * k1 * k3;
            if (discriminant < 0)
            {
                return (double.PositiveInfinity, double.PositiveInfinity);
            }

            var t1 = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
            var t2 = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
            return (t1, t2);
        }

        private double _lenghtToNearBorder = 1;
    }

    public class Sphere
    {
        public Point3D Center;
        public double Radius;
        public Color Color;

        public Sphere(Point3D center, double radius, Color color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }
    }
}