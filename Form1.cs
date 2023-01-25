using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace RayTracing2._0 {
    public partial class Form1 : Form {
        private readonly Timer _timer = new Timer() { Interval = 1 };
        private readonly Scene _scene = new Scene();
        protected override bool DoubleBuffered { get; set; } = true;
        private int _angleY;
        private int _angleX;
        private bool _navigate;

        public Form1() {
            InitializeComponent();
            pictureBox.KeyDown += NavigateCamera;
            pictureBox.MouseMove += RotateCamera;
            pictureBox.MouseClick += ChangeNavigate;
            Size = new Size(200, 200);
            pictureBox.Resize += onImageResize;

            // _timer.Tick += async (sender, args) =>
            // {
            //     _timer.Enabled = false;
            //     var newCanvasSize = new Size(pictureBox.Width, pictureBox.Height);
            //     if (newCanvasSize.Width != 0 && newCanvasSize.Height != 0)
            //     {
            //         var image = await _scene.GetFrame(newCanvasSize);
            //         pictureBox.Image = image;
            //     }
            //     _timer.Enabled = true;
            // };
            //
            // _timer.Start();
            new Thread(UpdateFrame).Start();
        }

        public void onImageResize(object sender, EventArgs e) {
        }

        public void UpdateFrame() {
            var newCanvasSize = new Size(pictureBox.Width, pictureBox.Height);
            var fragmentsCount = newCanvasSize.Width;



            _ = _scene.GetFrame(newCanvasSize, fragmentsCount).ContinueWith(task => {
                UpdateFrame();
                // var canvas = new Bitmap(newCanvasSize.Width, newCanvasSize.Height);
                // var fragments = task.Result;
                // var columnsPerFragment = canvas.Width / fragmentsCount + 1;
                // foreach (var imageFragment in fragments)
                // {
                //     var colors = imageFragment.Colors;
                //     var fragmentShift = imageFragment.FragmentIndex * columnsPerFragment;
                //     for (var x = 0; x < colors.GetLength(0); x++)
                //     {
                //         var dx = x + fragmentShift;
                //         if(dx >= canvas.Width)
                //             break;
                //         for (var y = 0; y < colors.GetLength(1); y++)
                //         {
                //             
                //             canvas.SetPixel(dx, y, colors[x, y]);
                //         }
                //     }
                // }
                var image = task.Result;

                new Thread(() => {
                    Bitmap bitmap = new(width: newCanvasSize.Width, height: newCanvasSize.Height);
                    using(Graphics g = Graphics.FromImage(bitmap)) {
                        g.Clear(Color.SkyBlue);
                    }

                    foreach(var pair in image) {
                        var color = pair.Value?.ToColor();
                        bitmap.SetPixel(pair.Key.X, pair.Key.Y, color: color ?? Color.SkyBlue);
                    }
                    pictureBox.Invoke(new MyDelegate(UpdateImage), bitmap);
                }).Start();

            });
        }

        public delegate void MyDelegate(Bitmap bitmap);

        private void UpdateImage(Bitmap bitmap) {
            pictureBox.Image = bitmap;
        }

        private void ChangeNavigate(object sender, MouseEventArgs e) {
            _navigate = !_navigate;
            if(_navigate) {
                Point leftTop = pictureBox.PointToScreen(new Point(pictureBox.Width / 2, pictureBox.Height / 2));
                Cursor.Position = leftTop;
                Cursor.Hide();
            } else {
                Cursor.Show();
            }
        }

        private void RotateCamera(object sender, MouseEventArgs e) {
            if(!_navigate) {
                return;
            }

            var d = Math.PI / 180;

            var pictureBoxWidthCenter = pictureBox.Width / 2;
            var pictureBoxHeightCenter = pictureBox.Height / 2;

            _angleY += e.X - pictureBoxWidthCenter;
            _angleX += e.Y - pictureBoxHeightCenter;

            var rotationMatrix = Matrix4x4.Multiply(
                Matrix4x4.CreateRotationX((float)(_angleX * d)),
                Matrix4x4.CreateRotationY((float)(_angleY * d))

                );


            _scene.RotationMatrix = rotationMatrix;


            Point leftTop = pictureBox.PointToScreen(new Point(pictureBoxWidthCenter, pictureBoxHeightCenter));
            Cursor.Position = leftTop;
        }

        #region OldCode

        // private SizeF UpdateViewSize(int width, int height)
        // {
        //     return height > width
        //         ? new SizeF((float) width * 1 / height, 1)
        //         : new SizeF(1, (float) height * 1 / width);
        // }
        //
        // private Task<Bitmap> UpdateCanvasAsync(Size localCanvasSize)
        // {
        //     var task = new Task<Bitmap>(() =>
        //     {
        //         var camera = new Vector3(_camera.X, _camera.Y, _camera.Z);
        //         
        //         var canvas = new Bitmap(localCanvasSize.Width, localCanvasSize.Height, PixelFormat.Format32bppArgb);
        //         var canvasHeight = localCanvasSize.Height;
        //         var canvasWidth = localCanvasSize.Width;
        //
        //         // for(var x = -canvasWidth / 2; x <canvasWidth / 2; x++)
        //         // {
        //         //     var dx = x + canvasWidth / 2;
        //         //
        //         //
        //         //     for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
        //         //     {
        //         //         var viewPortPosition = CanvasToDirectionViewport(x, y);
        //         //         var result = TraceRay(camera, viewPortPosition, 0.001, double.PositiveInfinity, 3);
        //         //         if(result.Success)
        //         //         {
        //         //             var dy = canvasHeight / 2 - y;
        //         //             var color = result.VecColor.ToColor();
        //         //
        //         //             lock (canvas)
        //         //             {
        //         //                 canvas.SetPixel(dx, dy, color);
        //         //             }
        //         //         }
        //         //     }
        //         // }
        //
        //         Parallel.For(-canvasWidth / 2, canvasWidth / 2, x =>
        //         {
        //             var dx = x + canvasWidth / 2;
        //         
        //         
        //             for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
        //             {
        //                 var directionViewPortPosition = CanvasToDirectionViewport(x, y);
        //                 var result = TraceRay(camera, directionViewPortPosition, 0.001, double.PositiveInfinity, 4);
        //                 if(result.Success)
        //                 {
        //                     var dy = canvasHeight / 2 - y;
        //                     var color = result.VecColor.ToColor();
        //                     lock (canvas)
        //                     {
        //                        canvas.SetPixel(dx, dy, color);
        //                     }
        //                 }
        //             }
        //         });
        //
        //         return canvas;
        //     });
        //     task.Start();
        //     return task;
        // }
        //
        // private RayTraceResult TraceRay(Vector3 rayStart, Vector3 rayDirection, double iterationsLeft, 
        //     double tMin = 0.00001,
        //     double tMax = double.PositiveInfinity)
        // {
        //     if (iterationsLeft == 0)
        //         return RayTraceResult.Fail;
        //     
        //     iterationsLeft--;
        //
        //     var ray = new Ray(rayStart, rayDirection);
        //     var (crossObject, crossPoint) = FindCrossWithScene(new SearchParameters(ray, tMin, tMax));
        // }
        //
        // private RayTraceResult TraceRay(Vector3 fromPoint, Vector3 directionPoint, double tMin, double tMax, int iterationCount)
        // {
        //     if (iterationCount == 0)
        //         return RayTraceResult.Fail;
        //     
        //     iterationCount--;
        //     
        //     var (crossObject, crossPoint) = FindCrossWithScene(new SearchParameters(new Ray(fromPoint, directionPoint), tMin, tMax));
        //
        //     if (crossObject == null)
        //         return RayTraceResult.Fail;
        //
        //     var normalUnitVector = crossObject.GetNormalUnitVector(crossPoint);
        //     
        //     var (diffusedLight, primaryIllumination) = FindDiffusedAndPrimaryLight(normalUnitVector, crossPoint, crossObject, iterationCount);
        //     
        //     var reflectLight = FindReflectLight(normalUnitVector, directionPoint * -1, crossPoint, tMin, tMax, iterationCount);
        //     
        //     var resultColor = new ReflectionLight(
        //         diffusedLight, 
        //         reflectLight,
        //         primaryIllumination);
        //     return new RayTraceResult(true, crossObject, crossPoint, resultColor);
        // }
        //
        // private (ISceneObject, Vector3) FindCrossWithScene(SearchParameters parameters)
        // {
        //     ISceneObject sceneObject = null;
        //     var tMax = parameters.MaxRayCoefficient;
        //     foreach (var sphere in _spheres)
        //     {
        //         foreach (var rayCoef in sphere.FindIntersectsRay(parameters.Ray))
        //         {
        //             if (IsValidLength(parameters.MinRayCoefficient, tMax, rayCoef))
        //             {
        //                 tMax = rayCoef;
        //                 sceneObject = sphere;
        //             }
        //         }
        //     }
        //
        //     if (sceneObject == null)
        //         return (null, null);
        //     
        //     return (sceneObject, parameters.Ray.GetPointFromCoefficient(tMax));
        // }
        //
        // private static bool IsValidLength(double tMin, double tMax, double t)
        // {
        //     return t > tMin && t < tMax;
        // }
        //
        // private (VecColor, VecColor) FindDiffusedAndPrimaryLight(Vector3 normalVector3, Vector3 crossPoint, ISceneObject sceneObject, int remainingIterations)
        // {
        //     var diffusedLight = VecColor.Empty;
        //     var primaryIlluminationLight = VecColor.Empty;
        //     foreach (var light in _lights)
        //     {
        //         var lightDirection = light.GetDirection(crossPoint);
        //         var normalLightUnitVector = lightDirection / lightDirection.Lenght;
        //         var cosBetween = Vector3.DotProduct(normalVector3, normalLightUnitVector);
        //         if(cosBetween <= 0/* || AnyObjectBetween(crossPoint, light.Location, sceneObject)*/)
        //             continue;
        //         var (crossObject, _) = FindCrossWithScene(light.GetSearchParametersForEclipsingObjects(crossPoint));
        //         
        //         if (crossObject != null)
        //             continue;
        //         
        //         
        //         // var rayTraceResult = TraceRay(crossPoint, light.Location, 0.01, lightNormalVector.Lenght,
        //         //     1);
        //         // if(rayTraceResult.Success)
        //         //     continue;
        //         // VecColor lightIntensity;
        //         // if (rayTraceResult.Success)
        //         // {
        //         //     lightIntensity = rayTraceResult.ReflectionLight.ReflectColor * cosBetween;
        //         // }
        //         // else
        //         // {
        //         //     lightIntensity = light.Intensity * light.Color * cosBetween;
        //         // }
        //         var lightColor = light.Intensity * light.Color;
        //         var lightIntensity = lightColor * cosBetween;
        //         primaryIlluminationLight += lightColor * Math.Pow(cosBetween, sceneObject.SpecularCoef);
        //         diffusedLight += lightIntensity;
        //     }
        //
        //     return (GlobalLightningParameters.DiffusionIlluminationCoefficient * diffusedLight, primaryIlluminationLight);
        // }
        //
        // private VecColor FindReflectLight(Vector3 normalUnitVector3, Vector3 negativeRayDirection, Vector3 crossPoint, double tMin, double tMax, int iterationCount) 
        // {
        //     // var normalizeCrossPoint = negativeRayDirection / negativeRayDirection.Lenght;
        //     // var newPoint = normalizeCrossPoint - 2.0 * normalUnitVector3 * Vector3.DotProduct(normalUnitVector3, normalizeCrossPoint);
        //     // var directionPoint = newPoint - negativeRayDirection;
        //     var reflectDirection = 2*normalUnitVector3*Vector3.DotProduct(normalUnitVector3, negativeRayDirection) - negativeRayDirection;
        //     var rayTraceResult = TraceRay(crossPoint, reflectDirection, tMin, tMax, iterationCount);
        //     
        //     if (rayTraceResult.Success)
        //     {
        //         return rayTraceResult.VecColor
        //             /** GlobalLightningParameters.ReflectedBeamRatio 
        //             * Math.Pow(Math.E, -1 * (crossPoint - rayTraceResult.CrossPoint).Lenght)*/;
        //     }
        //     
        //     return VecColor.Empty;
        // }

        #endregion

        private void pictureBox_Click(object sender, EventArgs e) {
            pictureBox.Focus();
        }

        private void NavigateCamera(object sender, KeyEventArgs e) {
            if(!_navigate)
                return;

            var d = 0.2f;
            var camera = _scene.CameraPosition;
            switch(e.KeyCode) {
                case Keys.A: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(-d, 0, 0), _scene.RotationMatrix) + camera;
                        return;
                    }
                case Keys.D: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(d, 0, 0), _scene.RotationMatrix) + camera;
                        return;
                    }
                case Keys.W: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(0, 0, d), _scene.RotationMatrix) + camera;
                        return;
                    }
                case Keys.S: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(0, 0, -d), _scene.RotationMatrix) + camera;
                        return;
                    }

                case Keys.Space: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(0, d, 0), _scene.RotationMatrix) + camera;
                        return;
                    }

                case Keys.ShiftKey: {
                        _scene.CameraPosition = Vector3.Transform(new Vector3(0, -d, 0), _scene.RotationMatrix) + camera;
                        return;
                    }
            }
        }

        #region Not work

        // private bool AnyObjectBetween(Vector3 firstPoint, Vector3 secondPoint, ISceneObject sceneObject)
        // {
        //     var cos = (float) Vector3.CosBetween(firstPoint, secondPoint);
        //     var ray = new Ray(firstPoint, secondPoint - firstPoint);
        //     foreach (var sphere in _spheres)
        //     {
        //         foreach (var crossCoef in sphere.FindIntersectsRay(ray))
        //         {
        //             // var crossPoint  = firstPoint + secondPoint * crossCoef;
        //             // if (PointBetween(crossPoint, firstPoint, secondPoint, cos))
        //             //     return true;
        //             if (crossCoef >= 0 && crossCoef <= 1)
        //                 return true;
        //         }
        //     }
        //
        //     return false;
        // }
        //
        // private bool PointBetween(Vector3 crossPoint, Vector3 firstPoint, Vector3 secondPoint, float cos)
        // {
        //     var cosBetween = (float) Vector3.CosBetween(crossPoint, firstPoint);
        //     var between = (float) Vector3.CosBetween(crossPoint, secondPoint);
        //     return cos <= cosBetween && cos <= between;
        // }

        #endregion
    }
}