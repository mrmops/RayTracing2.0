using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        private Vector3 _camera = new Vector3(0, 0, 0);
        private List<Sphere> _spheres = new List<Sphere>()
        {
            new Sphere(new Vector3(0, -1, 3), 1, VecColor.FromColor(Color.Red)),
            new Sphere(new Vector3(-2, 0, 4), 1, VecColor.FromColor(Color.Blue)),
            new Sphere(new Vector3(2, 0, 4), 1, VecColor.FromColor(Color.Purple)),
            new Sphere(new Vector3(0, -5001, 0), 5000, VecColor.FromColor(Color.Yellow)),
        };

        private List<Light> _lights = new List<Light>()
        {
            //new Light(new Vector3(4, 2, 0), VecColor.FromColor(Color.White), 0.6),
            new PointLight(new Vector3(-4, 2, 0), VecColor.FromColor(Color.LightYellow), 0.6),
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.2),
            //new Light(new Vector(-1, 2, -1), VecColor.FromColor(Color.LightYellow), 0.75)
        };

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pictureBox.KeyDown += NavigateCamera;
            
            _canvasSize = new Size(pictureBox.Width, pictureBox.Height);
             _viewMatrix =UpdateViewSize(_canvasSize.Width, _canvasSize.Height);
             pictureBox.SizeChanged += (sender, args) =>
            {
                _canvasSize = new Size(pictureBox.Width, pictureBox.Height);
                _viewMatrix = UpdateViewSize(_canvasSize.Width, _canvasSize.Height);
                if (UpdateIsStoped && _canvasSize.Width != 0 && _canvasSize.Height != 0)
                {
                    UpdateIsStoped = false;
                }
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
                var camera = new Vector3(_camera.X, _camera.Y, _camera.Z);
                
                var canvas = new Bitmap(localCanvasSize.Width, localCanvasSize.Height, PixelFormat.Format32bppArgb);
                var canvasHeight = localCanvasSize.Height;
                var canvasWidth = localCanvasSize.Width;

                // for(var x = -canvasWidth / 2; x <canvasWidth / 2; x++)
                // {
                //     var dx = x + canvasWidth / 2;
                //
                //
                //     for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                //     {
                //         var viewPortPosition = CanvasToDirectionViewport(x, y);
                //         var result = TraceRay(camera, viewPortPosition, 0.001, double.PositiveInfinity, 3);
                //         if(result.Success)
                //         {
                //             var dy = canvasHeight / 2 - y;
                //             var color = result.VecColor.ToColor();
                //
                //             lock (canvas)
                //             {
                //                 canvas.SetPixel(dx, dy, color);
                //             }
                //         }
                //     }
                // }

                Parallel.For(-canvasWidth / 2, canvasWidth / 2, x =>
                {
                    var dx = x + canvasWidth / 2;
                
                
                    for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                    {
                        var directionViewPortPosition = CanvasToDirectionViewport(x, y);
                        var result = TraceRay(camera, directionViewPortPosition, 0.001, double.PositiveInfinity, 4);
                        if(result.Success)
                        {
                            var dy = canvasHeight / 2 - y;
                            var color = result.VecColor.ToColor();
                            lock (canvas)
                            {
                               canvas.SetPixel(dx, dy, color);
                            }
                        }
                    }
                });

                return canvas;
            });
            task.Start();
            return task;
        }
        
        private Vector3 CanvasToDirectionViewport(int x, int y)
        {
            return new Vector3(
                x * (double) _viewMatrix.Width / _canvasSize.Width,
                y * (double)_viewMatrix.Height / _canvasSize.Height,
                _lenghtToNearBorder
            );
        }

        private RayTraceResult TraceRay(Vector3 fromPoint, Vector3 directionPoint, double tMin, double tMax, int iterationCount)
        {
            if (iterationCount == 0)
                return RayTraceResult.Fail;
            
            iterationCount--;
            
            var (crossObject, crossPoint) = FindCrossWithScene(new SearchParameters(new Ray(fromPoint, directionPoint), tMin, tMax));

            if (crossObject == null)
                return RayTraceResult.Fail;

            var normalUnitVector = crossObject.GetNormalUnitVector(crossPoint);
            
            var (diffusedLight, primaryIllumination) = FindDiffusedAndPrimaryLight(normalUnitVector, crossPoint, crossObject, iterationCount);
            
            var reflectLight = FindReflectLight(normalUnitVector, directionPoint * -1, crossPoint, tMin, tMax, iterationCount);
            
            var resultColor = new ReflectionLight(
                diffusedLight, 
                reflectLight,
                primaryIllumination);
            return new RayTraceResult(true, crossObject, crossPoint, resultColor);
        }
        
        private (ISceneObject, Vector3) FindCrossWithScene(SearchParameters parameters)
        {
            ISceneObject sceneObject = null;
            var tMax = parameters.Max;
            foreach (var sphere in _spheres)
            {
                foreach (var rayCoef in sphere.FindIntersectsRay(parameters.Ray))
                {
                    if (IsValidLength(parameters.Min, tMax, rayCoef))
                    {
                        tMax = rayCoef;
                        sceneObject = sphere;
                    }
                }
            }

            if (sceneObject == null)
                return (null, null);
            
            return (sceneObject, parameters.Ray.GetPointFromCoefficient(tMax));
        }
        
        private static bool IsValidLength(double tMin, double tMax, double t)
        {
            return t > tMin && t < tMax;
        }

        private (VecColor, VecColor) FindDiffusedAndPrimaryLight(Vector3 normalVector3, Vector3 crossPoint, ISceneObject sceneObject, int remainingIterations)
        {
            var diffusedLight = VecColor.Empty;
            var primaryIlluminationLight = VecColor.Empty;
            foreach (var light in _lights)
            {
                var lightDirection = light.GetDirection(crossPoint);
                var normalLightUnitVector = lightDirection / lightDirection.Lenght;
                var cosBetween = Vector3.DotProduct(normalVector3, normalLightUnitVector);
                if(cosBetween <= 0/* || AnyObjectBetween(crossPoint, light.Location, sceneObject)*/)
                    continue;
                var (crossObject, _) = FindCrossWithScene(light.GetSearchParametersForEclipsingObjects(crossPoint));
                
                if (crossObject != null)
                    continue;
                
                
                // var rayTraceResult = TraceRay(crossPoint, light.Location, 0.01, lightNormalVector.Lenght,
                //     1);
                // if(rayTraceResult.Success)
                //     continue;
                // VecColor lightIntensity;
                // if (rayTraceResult.Success)
                // {
                //     lightIntensity = rayTraceResult.ReflectionLight.ReflectColor * cosBetween;
                // }
                // else
                // {
                //     lightIntensity = light.Intensity * light.Color * cosBetween;
                // }
                var lightColor = light.Intensity * light.Color;
                var lightIntensity = lightColor * cosBetween;
                primaryIlluminationLight += lightColor * Math.Pow(cosBetween, sceneObject.SpecularCoef);
                diffusedLight += lightIntensity;
            }

            return (GlobalLightningParameters.DiffusionIlluminationCoefficient * diffusedLight, primaryIlluminationLight);
        }

        private VecColor FindReflectLight(Vector3 normalUnitVector3, Vector3 negativeRayDirection, Vector3 crossPoint, double tMin, double tMax, int iterationCount) 
        {
            // var normalizeCrossPoint = negativeRayDirection / negativeRayDirection.Lenght;
            // var newPoint = normalizeCrossPoint - 2.0 * normalUnitVector3 * Vector3.DotProduct(normalUnitVector3, normalizeCrossPoint);
            // var directionPoint = newPoint - negativeRayDirection;
            var reflectDirection = 2*normalUnitVector3*Vector3.DotProduct(normalUnitVector3, negativeRayDirection) - negativeRayDirection;
            var rayTraceResult = TraceRay(crossPoint, reflectDirection, tMin, tMax, iterationCount);
            
            if (rayTraceResult.Success)
            {
                return rayTraceResult.VecColor
                    /** GlobalLightningParameters.ReflectedBeamRatio 
                    * Math.Pow(Math.E, -1 * (crossPoint - rayTraceResult.CrossPoint).Lenght)*/;
            }
            
            return VecColor.Empty;
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
                    _camera = new Vector3(_camera.X  - d, _camera.Y, _camera.Z);
                    return;
                }
                case Keys.D:
                {
                    _camera = new Vector3(_camera.X  + d, _camera.Y, _camera.Z);
                    return;
                }
                case Keys.W:
                {
                    _camera = new Vector3(_camera.X, _camera.Y, _camera.Z  + d);
                    return;
                }
                case Keys.S:
                {
                    _camera = new Vector3(_camera.X, _camera.Y, _camera.Z  - d);
                    return;
                }
                
                case Keys.Space:
                {
                    _camera = new Vector3(_camera.X, _camera.Y + d, _camera.Z);
                    return;
                }
                
                case Keys.ShiftKey:
                {
                    _camera = new Vector3(_camera.X, _camera.Y - d, _camera.Z);
                    return;
                }
            }
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {
            GlobalLightningParameters.BackgroundIlluminationRatio = trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            GlobalLightningParameters.DiffusionIlluminationCoefficient = trackBar3.Value / 50d;
        }


        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            GlobalLightningParameters.ReflectedBeamRatio = trackBar4.Value / 50d;
        }
        
        #region Not work

        private bool AnyObjectBetween(Vector3 firstPoint, Vector3 secondPoint, ISceneObject sceneObject)
        {
            var cos = (float) Vector3.CosBetween(firstPoint, secondPoint);
            var ray = new Ray(firstPoint, secondPoint - firstPoint);
            foreach (var sphere in _spheres)
            {
                foreach (var crossCoef in sphere.FindIntersectsRay(ray))
                {
                    // var crossPoint  = firstPoint + secondPoint * crossCoef;
                    // if (PointBetween(crossPoint, firstPoint, secondPoint, cos))
                    //     return true;
                    if (crossCoef >= 0 && crossCoef <= 1)
                        return true;
                }
            }

            return false;
        }

        private bool PointBetween(Vector3 crossPoint, Vector3 firstPoint, Vector3 secondPoint, float cos)
        {
            var cosBetween = (float) Vector3.CosBetween(crossPoint, firstPoint);
            var between = (float) Vector3.CosBetween(crossPoint, secondPoint);
            return cos <= cosBetween && cos <= between;
        }

        #endregion
    }
}