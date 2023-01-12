using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using RayTracing2._0.Material;
using RayTracing2._0.SceneObjects;

namespace RayTracing2._0
{
    public class Scene
    {
        private ConcurrentDictionary<Vector3, ConcurrentDictionary<Vector3, TraceRayResult>> _cache =
            new ConcurrentDictionary<Vector3, ConcurrentDictionary<Vector3, TraceRayResult>>();

        private List<ISceneObject> _sceneObjects = new List<ISceneObject>()
        {
            new Sphere(new Vector3(0, -1, 2), 1,
                new CustomMaterial(VecColor.FromColor(Color.Blue),
                    0.3, 500, 2.5, 0.5)),
            new Lens(
                /*new CustomMaterial(VecColor.FromColor(Color.Yellow), 0, 100, 0,
                    1)*/ new Glass(VecColor.FromColor(Color.Pink), 100, 0), new Vector3(0, 0, 7), 1),

            new Sphere(new Vector3(-2, 0, 4), 1,
                new Glass(VecColor.FromColor(Color.Red), 50, 0.02)),

            new Cube(new Glass(VecColor.FromColor(Color.Yellow/*Color.FromArgb(255, 110, 0 , 150)*/), 100, 0.05), 1),
            
            new Prism(2,new Glass(VecColor.FromColor(Color.HotPink/*Color.FromArgb(255, 110, 0 , 150)*/), 100, 0)),

            new Sphere(new Vector3(2, 0, 4), 1,
                new CustomMaterial(VecColor.FromColor(Color.Green), 1, 500, 0, 1)),

            new Sphere(new Vector3(0, -5001, 0), 5000,
                new CustomMaterial(VecColor.FromColor(Color.Yellow), 0, 100, 0, 1)),
        };

        public Vector3 CameraPosition = new Vector3(0, 0, 0);
        public Matrix RotationMatrix = Matrix.CreateStartMatrix();
        private double _lenghtToNearBorder = 1;

        private List<Light> _lights = new List<Light>()
        {
            new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6),
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.4),
        };

        public Task<VecColor[,]> GetFrame(Size frameSize, int fragmentsCount)
        {
            var task = new Task<VecColor[,]>(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);

                //var canvas = new Bitmap(frameSize.Width, frameSize.Height, PixelFormat.Format32bppArgb);
                var canvasHeight = frameSize.Height;
                var canvasWidth = frameSize.Width;
                VecColor[,] canvasColors = new VecColor[canvasWidth, canvasHeight];

                var stepToPixel = 1 / (double) canvasHeight;
                //var fragments = new ConcurrentBag<ImageFragment>();

                // for(var x = -canvasWidth / 2; x <canvasWidth / 2; x++)
                // {
                //     var dx = x + canvasWidth / 2;
                //     
                //     for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                //     {
                //         var directionViewPortPosition = RotationMatrix * CanvasToDirectionViewport(x, y, stepToPixel);
                //         var result = TraceRay(camera, directionViewPortPosition, 3);
                //         if(result.Success)
                //         {
                //             var dy = canvasHeight / 2 - y;
                //             var color = result.ResultColor.ToColor();
                //             // lock (canvas)
                //             // {
                //                 canvas.SetPixel(dx, dy, color);
                //             //}
                //         }
                //     }
                // }

                var columnsPerFragment = canvasWidth / fragmentsCount + 1;

                ConcurrentQueue<int> fragments = new ConcurrentQueue<int>(Enumerable.Range(0, fragmentsCount));

                Parallel.For(0, Math.Min(fragmentsCount, 256), _ =>
                {
                    while (true) {
                        var success = fragments.TryDequeue(out int fragmentIndex);

                        if (!success)
                        {
                            return;
                        }

                        //var colors = new Color[columnsPerFragment, canvasHeight];
                        var fragmentShift = fragmentIndex * columnsPerFragment;
                        for (var localX = 0; localX < columnsPerFragment; localX++)
                        {
                            var dx = localX + fragmentShift;
                            if (dx >= canvasWidth)
                                return;
                            var x = dx - canvasWidth / 2;
                            var halfCanvasHeight = canvasHeight / 2;
                            for (var y = -halfCanvasHeight + 1; y < halfCanvasHeight; y++)
                            {
                                var directionViewPortPosition =
                                    RotationMatrix * CanvasToDirectionViewport(x, y, stepToPixel);
                                var result = TraceRay(camera, directionViewPortPosition, 5);
                                var dy = halfCanvasHeight - y;
                                var color = result.ResultColor ?? VecColor.Empty;
                                //colors[localX, dy] = color;
                                canvasColors[dx, dy] = color;

                                /*lock(fragments)
                                    canvas.SetPixel(dx, dy, color);
                                var a = 10;*/

                                /*if(result.Success)
                                {


                                    //dict.Add(new Point(dx,dy), color);
                                    // lock (canvas)
                                    // {
                                    //     canvas.SetPixel(dx, dy, color);
                                    // }
                                }*/
                            }
                        }

                        //fragments.Add(new ImageFragment(fragmentIndex, colors));

                        //  foreach (var colorLocation in dict)
                        //  {
                        //      var point = colorLocation.Key;
                        //      canvas.SetPixel(point.X, point.Y, colorLocation.Value);
                        // }

                        // lock(fragments)
                        //     for (var x = 0; x < colors.GetLength(0); x++)
                        //     {
                        //         var dx = x + fragmentShift;
                        //         if(dx >= canvas.Width)
                        //             break;
                        //         for (var y = 0; y < colors.GetLength(1); y++)
                        //         {
                        //             canvas.SetPixel(dx, y, colors[x, y]);
                        //         }
                        //     }
                    }
                });

                // for (int x = 0; x < canvasWidth; x++)
                // {
                //     for (int y = 0; y < canvasHeight; y++)
                //     {
                //         camera.SetPixel(x, y, taskResult[x,y]);
                //     }
                // }
                stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
                return canvasColors;
            });
            task.Start();
            return task;
        }


        private Vector3 CanvasToDirectionViewport(int x, int y, double stepToPixel)
        {
            return new Vector3(
                x * stepToPixel,
                y * stepToPixel,
                _lenghtToNearBorder
            );
        }

        private TraceRayResult TraceRay(Vector3 rayStart, Vector3 directionVector, int iterationsLeft,
            double minRayCoefficient = 0.0000001,
            double maxRayCoefficient = double.PositiveInfinity)
        {
            if (iterationsLeft == 0)
                return TraceRayResult.Fail();
            iterationsLeft--;

            var ray = new Ray(rayStart, directionVector);
            // if (_cache.TryGetValue(rayStart, out var subCache))
            // {
            //     if (subCache.TryGetValue(directionVector, out var result))
            //     {
            //         return result;
            //     }
            // }
            // else
            // {
            //     _cache.AddOrUpdate(rayStart, new ConcurrentDictionary<Vector3, TraceRayResult>(), (vector3, results1) => results1);
            // }
            var searchParameters =
                new SearchParameters(ray, minRayCoefficient, maxRayCoefficient);

            TraceRayResult result1;
            if (TryGetTheClosestIntersectionWithTheScene(searchParameters,
                out var intersectedObject,
                out var intersectedPoint,
                out var normalUnitVector))
            {
                var (diffusedLight, primaryLight) = FindDiffusedAndPrimaryLight(intersectedObject,
                    intersectedPoint, normalUnitVector, iterationsLeft);

                var reflectLight = intersectedObject.Material.ReflectCoef != 0
                    ? FindReflectLight(intersectedPoint, normalUnitVector, directionVector, iterationsLeft)
                    : VecColor.Empty;

                var refractedLight = intersectedObject.Material.AbsorptionCoefficient < 1
                    ? FindRefractedLight(intersectedObject, intersectedPoint, normalUnitVector,
                        directionVector / directionVector.Lenght, iterationsLeft)
                    : VecColor.Empty;

                var reflectCoef = intersectedObject.Material.ReflectCoef;

                var absorptionCoefficient = intersectedObject.Material.AbsorptionCoefficient;
                var resultColor = reflectLight * reflectCoef
                                  + ((diffusedLight
                                      + primaryLight
                                      + GlobalLightningParameters.BackgroundLight) * (absorptionCoefficient)
                                     + refractedLight * (1 - absorptionCoefficient)) * (1 - reflectCoef);


                result1 = new TraceRayResult(true, intersectedObject, intersectedPoint, resultColor);
            }
            else
                result1 = TraceRayResult.Fail();

            // if (_cache.TryGetValue(rayStart, out var dictionary))
            // {
            //     dictionary.AddOrUpdate(directionVector, result1, (vector3, result) => result);
            // }
            // else
            // {
            //     _cache.AddOrUpdate(rayStart, new ConcurrentDictionary<Vector3, TraceRayResult>(){
            //         [directionVector] = result1
            //     }, (vector3, results) =>
            //     {
            //         results[rayStart] = result1;
            //         return results;
            //     });
            // }
            return result1;
        }

        private VecColor FindRefractedLight(ISceneObject sceneObject, Vector3 intersectedPoint,
            Vector3 normalUnitVector, Vector3 rayDirectionVector, int iterationsLeft)
        {
            var localDirection = rayDirectionVector;
            var refractiveCoef = sceneObject.Material.RefractiveCoef;
            var dotProduct = Vector3.DotProduct(localDirection, normalUnitVector);
            Vector3 refractiveDirection;
            if (dotProduct <= 0)
            {
                refractiveDirection = localDirection +
                                      (Math.Sqrt(
                                           (refractiveCoef * refractiveCoef - 1)
                                           / dotProduct * dotProduct + 1)
                                       - 1)
                                      * dotProduct
                                      * normalUnitVector;
            }
            else
            {
                var newCoef = 1 / refractiveCoef;
                refractiveDirection = localDirection +
                                      (Math.Sqrt(
                                           (newCoef * newCoef - 1)
                                           / dotProduct * dotProduct + 1)
                                       - 1)
                                      * dotProduct
                                      * normalUnitVector;
            }

            var traceRayResult = TraceRay(intersectedPoint, refractiveDirection, iterationsLeft);
            var lenghtToIntersectPoint = traceRayResult.Success
                ? (intersectedPoint - traceRayResult.IntersectedPoint).Lenght
                : 0;
            return VecColor.Intersection(traceRayResult.ResultColor, sceneObject.Material.Color)
                   * Math.Pow(Math.E, -1 * sceneObject.Material.AbsorptionCoefficient * lenghtToIntersectPoint);
        }

        private VecColor FindReflectLight(Vector3 intersectedPoint, Vector3 normalUnitVector,
            Vector3 rayDirectionVector, int iterationsLeft)
        {
            var negativeRayDirection = -1 * rayDirectionVector;
            var reflectDirection = 2 * normalUnitVector * Vector3.DotProduct(normalUnitVector, negativeRayDirection) -
                                   negativeRayDirection;
            var rayTraceResult = TraceRay(intersectedPoint, reflectDirection, iterationsLeft);

            if (rayTraceResult.Success)
            {
                var lenghtToIntersect = (intersectedPoint - rayTraceResult.IntersectedPoint).Lenght;
                return rayTraceResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
            }

            return rayTraceResult.ResultColor /** Math.Pow(Math.E, -1)*/;
        }

        private bool TryGetTheClosestIntersectionWithTheScene(SearchParameters parameters,
            out ISceneObject intersectedObject,
            out Vector3 intersectedPoint,
            out Vector3 normal)
        {
            ISceneObject sceneObject = null;
            var localMaximum = parameters.MaxRayCoefficient;
            normal = new Vector3(0, 1, 0);
            foreach (var obj in _sceneObjects)
            {
                foreach (var rayCoef in obj.FindIntersectedRayCoefficients(parameters.Ray))
                {
                    if (IsValidLength(parameters.MinRayCoefficient, localMaximum, rayCoef.Item1))
                    {
                        localMaximum = rayCoef.Item1;
                        sceneObject = obj;
                        normal = rayCoef.Item2;
                    }
                }
            }

            if (sceneObject == null)
            {
                intersectedObject = null;
                intersectedPoint = null;
                return false;
            }

            intersectedObject = sceneObject;
            intersectedPoint = parameters.Ray.GetPointFromCoefficient(localMaximum);

            return true;
        }

        private static bool IsValidLength(double tMin, double tMax, double t)
        {
            return t > tMin && t < tMax;
        }

        private (VecColor, VecColor) FindDiffusedAndPrimaryLight(ISceneObject sceneObject, Vector3 pointToObject,
            Vector3 normalUnitVectorToPoint, int iterationsLeft)
        {
            var resultDiffused = VecColor.Empty;
            var resultPrimary = VecColor.Empty;
            foreach (var light in _lights)
            {
                var direction = light.GetDirection(pointToObject);
                var unitDirection = direction / direction.Lenght;
                var cos = Vector3.DotProduct(normalUnitVectorToPoint, unitDirection);
                if (cos <= 0)
                {
                    continue;
                }

                var traceResult = TraceRay(pointToObject, direction, iterationsLeft, maxRayCoefficient: 1);
                VecColor lightColor;
                if (traceResult.Success)
                {
                    lightColor = traceResult.ResultColor;
                }
                else
                {
                    lightColor = light.Color * light.Intensity;
                    resultPrimary += lightColor * Math.Pow(cos, sceneObject.Material.SpecularCoef);
                }

                resultDiffused += lightColor * cos;
            }

            /*foreach (var o in _sceneObjects)
            {
                var traceRayResult = TraceRay(pointToObject, (o.Location - pointToObject).Normalized(), iterationsLeft,
                    maxRayCoefficient: 5);
                if(traceRayResult.Success)
                {
                    var lenghtToIntersect = (traceRayResult.IntersectedPoint - traceRayResult.IntersectedPoint).Lenght;
                    resultDiffused += traceRayResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
                }
            }*/
            return (VecColor.Intersection(resultDiffused, sceneObject.Material.Color), resultPrimary);
        }
    }

    public class ImageFragment
    {
        public int FragmentIndex;
        public Color[,] Colors;

        public ImageFragment(int fragmentIndex, Color[,] colors)
        {
            FragmentIndex = fragmentIndex;
            Colors = colors;
        }
    }
}