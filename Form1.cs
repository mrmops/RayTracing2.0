using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace RayTracing2._0
{
    public partial class Form1 : Form
    {

        private Size _canvasSize;
        private Timer _timer = new Timer() {Interval = 1};
        private SizeF _viewMatrix;
        private Vector _camera = new Vector(0, 0, 0);
        private List<Sphere> _spheres = new List<Sphere>()
        {
            new Sphere(new Vector(0, -1, 3), 1, VecColor.FromColor(Color.Red)),
            new Sphere(new Vector(-2, 0, 4), 1, VecColor.FromColor(Color.Blue)),
            new Sphere(new Vector(2, 0, 4), 1, VecColor.FromColor(Color.Green)),
            new Sphere(new Vector(0, -5001, 0), 5000, VecColor.FromColor(Color.Yellow)),
        };

        private List<Light> _lights = new List<Light>()
        {
            new Light(new Vector(2, 1, 0), VecColor.FromColor(Color.White), 0.75),
            //new Light(new Vector(-1, 2, -1), VecColor.FromColor(Color.LightYellow), 0.75)
        };

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pictureBox.KeyDown += NavigateCamera;
            
            
            // WindowState = FormWindowState.Maximized;
            _canvasSize = new Size(pictureBox.Width, pictureBox.Height);
             _viewMatrix =UpdateViewSize(_canvasSize.Width, _canvasSize.Height);
            //_viewCanvas = new SizeF(2, 2);
            pictureBox.SizeChanged += (sender, args) =>
            {
                _canvasSize = new Size(pictureBox.Width, pictureBox.Height);
                _viewMatrix = UpdateViewSize(_canvasSize.Width, _canvasSize.Height);
            };
            _timer.Tick += async (sender, args) =>
            {
                _timer.Enabled = false;
                var newCanvasSize = new Size(_canvasSize.Width, _canvasSize.Height);
                if (_canvasSize.Width != 0 && _canvasSize.Height != 0)
                {
                    var image = await UpdateCanvasAsync(newCanvasSize);
                    pictureBox.Image = image;
                }
                _timer.Enabled = true;
            };
            
            _timer.Start();
        }

        private SizeF UpdateViewSize(int width, int height)
        {
            return height > width
                ? new SizeF((float) width * 1 / height, 1)
                : new SizeF(1, (float) height * 1 / width);
        }

        private Task<Bitmap> UpdateCanvasAsync(Size localCanvasSize)
        {
            var task = new Task<Bitmap>(() =>
            {
                var camera = new Vector(_camera.X, _camera.Y, _camera.Z);
                
                var canvas = new Bitmap(localCanvasSize.Width, localCanvasSize.Height);
                var canvasHeight = canvas.Height;

                Parallel.For(-canvas.Width / 2, canvas.Width / 2, x =>
                {
                    for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                    {
                        var viewPortPosition = CanvasToViewport(x, y) + camera;
                        var result = TraceRay(camera, viewPortPosition, 0.001, double.PositiveInfinity, 5);
                        if(result.Success)
                        {
                            lock (canvas)
                            {
                                canvas.SetPixel(x + canvas.Width / 2, canvas.Height / 2 - y, result.VecColor.ToColor());
                            }
                        }
                    }
                });
                return canvas;
            });
            task.Start();
            return task;
        }
        
        private Vector CanvasToViewport(int x, int y)
        {
            return new Vector(
                x * (double) _viewMatrix.Width / _canvasSize.Width,
                y * (double)_viewMatrix.Height / _canvasSize.Height,
                _lenghtToNearBorder
            );
        }

        private RayTraceResult TraceRay(Vector fromPoint, Vector directionPoint, double tMin, double tMax, int iterationCount)
        {
            if (iterationCount == 0)
                return RayTraceResult.Fail;
            
            iterationCount--;
            
            var crossObject = FindCrossObject(fromPoint, directionPoint, tMin, tMax, out var crossPoint);

            if (crossObject == null)
                return RayTraceResult.Fail;

            var normalUnitVector = crossObject.GetNormalUnitVector(crossPoint);
            var resultColor = GlobalLightningParameters.BackgroundLight
                              + FindDiffuseLight(normalUnitVector, crossPoint, crossObject)
                              + FindReflectLight(normalUnitVector, crossPoint, tMin, tMax, iterationCount);
            resultColor = resultColor ^ crossObject.Color;
            return new RayTraceResult(true, crossObject, crossPoint, resultColor);
        }

        private VecColor FindReflectLight(Vector normalUnitVector, Vector crossPoint, double tMin, double tMax, int iterationCount)
        {
            var normalizeCrossPoint = crossPoint / crossPoint.Lenght;
            var newPoint = normalizeCrossPoint - 2.0 * normalUnitVector * Vector.DotProduct(normalUnitVector, normalizeCrossPoint);
            var rayTraceResult = TraceRay(crossPoint, newPoint, tMin, tMax, iterationCount);
            
            if (rayTraceResult.Success)
            {
                return rayTraceResult.VecColor
                       * GlobalLightningParameters.ReflectedBeamRatio 
                       * Math.Pow(Math.E, -1 * (crossPoint - rayTraceResult.CrossPoint).Lenght);
            }
            
            return VecColor.Empty;
        }


        private VecColor FindDiffuseLight(Vector normalVector, Vector crossPoint, Sphere selectedSphere)
        {
            var sum = new VecColor(0, 0, 0);
            foreach (var light in _lights)
            {
                var lightNormalVector = light.Location - crossPoint;
                var normalLightUnitVector = lightNormalVector / lightNormalVector.Lenght;
                var cosBetween = Vector.DotProduct(normalVector, normalLightUnitVector);
                if(cosBetween <= 0 || AnyObjectBetween(crossPoint, light.Location, selectedSphere))
                    continue;
                var lightIntensity = light.Intensity * light.Color * (cosBetween >= 0 ? cosBetween : 0);
                
                sum =  sum + lightIntensity;
            }

            return GlobalLightningParameters.DiffusionIlluminationCoefficient * sum;
        }

        private bool AnyObjectBetween(Vector firstPoint, Vector secondPoint, Sphere selectedSphere)
        {
            var cos = (float) Vector.CosBetween(firstPoint, secondPoint);
            foreach (var sphere in _spheres)
            {
                if(sphere != selectedSphere && IntersectRaySphere(firstPoint, secondPoint, sphere, out var result))
                {
                    var minT = Math.Min(result.Item1, result.Item2);
                    var crossPoint  = firstPoint + secondPoint * minT;
                    if (PointBetween(crossPoint, firstPoint, secondPoint, cos))
                        return true;
                }
            }

            return false;
        }

        private bool PointBetween(Vector crossPoint, Vector firstPoint, Vector secondPoint, float cos)
        {
            var cosBetween = (float) Vector.CosBetween(crossPoint, firstPoint);
            var between = (float) Vector.CosBetween(crossPoint, secondPoint);
            if(cos <= cosBetween && cos <= between)
                Console.WriteLine();
            return cos < cosBetween && cos < between;
        }

        private Sphere FindCrossObject(Vector fromPoint, Vector toPoint, double tMin, double tMax, out Vector crossPoint)
        {
            Sphere closestSphere = null;
            var wasCross = false;
            foreach (var sphere in _spheres)
            {
                if(IntersectRaySphere(fromPoint, toPoint, sphere, out var result))
                {
                    var (t1, t2) = result;
                    if (ValidLength(tMin, tMax, t1))
                    {
                        tMax = t1;
                        closestSphere = sphere;
                        wasCross = true;
                    }

                    if (ValidLength(tMin, tMax, t2))
                    {
                        tMax = t2;
                        closestSphere = sphere;
                        wasCross = true;
                    }
                }
            }
            if(wasCross)
            {
                crossPoint = fromPoint + toPoint * tMax;
            }
            else
            {
                crossPoint = null;
            }

            return closestSphere;
        }

        private static bool ValidLength(double tMin, double tMax, double t)
        {
            return t >= tMin && t <= tMax && t < double.PositiveInfinity;
        }

        private bool IntersectRaySphere(Vector startRay, Vector pointOnRay, Sphere sphere, out (double, double) result)
        {
            var r = sphere.Radius;
            var oc = startRay - sphere.Center;

            var k1 = Vector.DotProduct(pointOnRay, pointOnRay);
            var k2 = 2 * Vector.DotProduct(oc, pointOnRay);
            var k3 = Vector.DotProduct(oc, oc) - r * r;

            var discriminant = k2 * k2 - 4 * k1 * k3;
            if (discriminant < 0)
            {
                result = (double.PositiveInfinity, double.PositiveInfinity);
                return false;
            }

            var t1 = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
            var t2 = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
            result = (t1, t2);
            return true;
        }

        private double _lenghtToNearBorder = 1;

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            _lenghtToNearBorder = trackBar1.Value / 10d;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            pictureBox.Focus();
        }
        
        
        private void NavigateCamera(object sender, KeyEventArgs e)
        {
            var d = 0.2;
            switch (e.KeyCode)
            {
                case Keys.A:
                {
                    _camera = new Vector(_camera.X - d, _camera.Y, _camera.Z);
                    return;
                }
                case Keys.D:
                {
                    _camera = new Vector(_camera.X + d, _camera.Y, _camera.Z);
                    return;
                }
                case Keys.W:
                {
                    _camera = new Vector(_camera.X, _camera.Y - d, _camera.Z);
                    return;
                }
                case Keys.S:
                {
                    _camera = new Vector(_camera.X , _camera.Y + d, _camera.Z);
                    return;
                }
            }
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {
            GlobalLightningParameters.BackgroundIlluminationRatio = trackBar2.Value / 50d;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            GlobalLightningParameters.DiffusionIlluminationCoefficient = trackBar3.Value / 50d;
        }


        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            GlobalLightningParameters.ReflectedBeamRatio = trackBar4.Value / 50d;
        }
    }

    internal class RayTraceResult
    {
        public bool Success;
        public Sphere CrossObject;
        public Vector CrossPoint;
        public VecColor VecColor;

        public RayTraceResult(bool success, Sphere crossObject = null, Vector crossPoint = null, VecColor vecColor = null)
        {
            Success = success;
            CrossObject = crossObject;
            CrossPoint = crossPoint;
            VecColor = vecColor;
        }

        public static RayTraceResult Fail => new RayTraceResult(false);
    }
}