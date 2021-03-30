using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using RayTracing2._0.Material;

namespace RayTracing2._0
{
    public class Scene
    {
        private List<ISceneObject> _sceneObjects = new List<ISceneObject>()
        {
            new Sphere(new Vector3(0, -1, 2), 1,new CustomMaterial(VecColor.FromColor(Color.Blue), 0.3, 500, 0, 1) ),
            new Sphere(new Vector3(-2, 0, 4), 1, new Glass(VecColor.FromColor(Color.Red), 100, 0)),
            new Sphere(new Vector3(2, 0, 4), 1, new CustomMaterial(VecColor.FromColor(Color.Green), 1, 500, 0, 1)),
            new Sphere(new Vector3(0, -5001, 0), 5000, new CustomMaterial(VecColor.FromColor(Color.Yellow), 0, 100, 0, 1)),
        };
        
        public Vector3 CameraPosition  = new Vector3(0, 0, 0);
        public Matrix RotationMatrix = Matrix.CreateStartMatrix();
        private double _lenghtToNearBorder = 1;

        private List<Light> _lights = new List<Light>()
        {
            new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6), 
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.4),
        };

        public Task<IEnumerable<ImageFragment>> GetFrame(Size frameSize, int fragmentsCount)
        {
            var task = new Task<IEnumerable<ImageFragment>>(() =>
            {
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                
                var canvas = new Bitmap(frameSize.Width, frameSize.Height, PixelFormat.Format32bppArgb);
                var canvasHeight = canvas.Height;
                var canvasWidth = canvas.Width;
                var stepToPixel = 1 / (double)canvasHeight;
                var fragments = new ConcurrentBag<ImageFragment>();

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

                Parallel.For(0, fragmentsCount, fragmentIndex =>
                {
                    var colors = new Color[columnsPerFragment, canvasHeight];
                    var fragmentShift = fragmentIndex * columnsPerFragment;
                    for (var localX = 0; localX < columnsPerFragment; localX++)
                    {
                        var dx = localX + fragmentShift;
                        if(dx >= canvasWidth)
                            break;
                        var x =  dx - canvasWidth / 2;
                        for (var y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                        {
                            var directionViewPortPosition = RotationMatrix * CanvasToDirectionViewport(x, y, stepToPixel);
                            var result = TraceRay(camera, directionViewPortPosition, 4);
                            var dy = canvasHeight / 2 - y;
                            var color = result.ResultColor?.ToColor() ?? Color.Black;
                            colors[localX, dy] = color;
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
                    fragments.Add(new ImageFragment(fragmentIndex, colors));
                    
                    // foreach (var colorLocation in dict)
                    // {
                    //     var point = colorLocation.Key;
                    //     canvas.SetPixel(point.X, point.Y, colorLocation.Value);
                    //}
                });

                // foreach (var imageFragment in fragments)
                // {
                //     var colors = imageFragment.Colors;
                //     var fragmentShift = imageFragment.FragmentIndex * columnsPerFragment;
                //     for (var x = 0; x < colors.GetLength(0); x++)
                //     {
                //         var dx = x + fragmentShift;
                //         if(dx >= canvasWidth)
                //             break;
                //         for (var y = 0; y < colors.GetLength(1); y++)
                //         {
                //             
                //             canvas.SetPixel(dx, y, colors[x, y]);
                //         }
                //     }
                // }

                return fragments;
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
            
            if(iterationsLeft == 0)
                return TraceRayResult.Fail();
            iterationsLeft--;

            var searchParameters =
                new SearchParameters(new Ray(rayStart, directionVector), minRayCoefficient, maxRayCoefficient);
            
            if(TryGetTheClosestIntersectionWithTheScene(searchParameters, 
                out var intersectedObject, 
                out var intersectedPoint))
            {
                var normalUnitVector = intersectedObject.GetNormalUnitVector(intersectedPoint);
                var (diffusedLight, primaryColor) = FindDiffusedAndPrimaryLight(intersectedObject, 
                    intersectedPoint, normalUnitVector, iterationsLeft);

                var reflectLight = intersectedObject.Material.ReflectCoef != 0 
                    ? FindReflectLight(intersectedPoint, normalUnitVector, directionVector, iterationsLeft)
                    : VecColor.Empty;
                
                 var refractedColor = intersectedObject.Material.AbsorptionCoefficient < 1
                     ? FindRefractedLight(intersectedObject, intersectedPoint, normalUnitVector, 
                     directionVector / directionVector.Lenght, iterationsLeft)
                     : VecColor.Empty;
                
                var reflectCoef = intersectedObject.Material.ReflectCoef; ; 
                var resultColor = reflectLight * reflectCoef 
                                  + diffusedLight * (1 - reflectCoef) 
                                  + primaryColor + GlobalLightningParameters.BackgroundLight
                                  + refractedColor;
                
                return new TraceRayResult(true, intersectedObject, intersectedPoint, resultColor);
            }
            
            return TraceRayResult.Fail();
        }

        private VecColor FindRefractedLight(ISceneObject sceneObject, Vector3 intersectedPoint, Vector3 normalUnitVector, Vector3 rayDirectionVector, int iterationsLeft)
        {
            var localDirection = rayDirectionVector;
            var refractiveCoef = sceneObject.Material.RefractiveCoef;
            var dotProduct = Vector3.DotProduct(localDirection, normalUnitVector);
            Vector3 refractiveDirection;
            if(dotProduct <= 0)
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

            var resultColor = TraceRay(intersectedPoint, refractiveDirection, iterationsLeft);
            if(!resultColor.Success)
                return VecColor.Empty;
            var lenghtToIntersectPoint = (intersectedPoint - resultColor.IntersectedPoint).Lenght;
            return 1
                   * resultColor.ResultColor
                   * Math.Pow(Math.E, -1 * sceneObject.Material.AbsorptionCoefficient * lenghtToIntersectPoint);
        }

        private VecColor FindReflectLight(Vector3 intersectedPoint, Vector3 normalUnitVector, Vector3 rayDirectionVector, int iterationsLeft)
        {
            var negativeRayDirection = -1 * rayDirectionVector;
            var reflectDirection = 2*normalUnitVector*Vector3.DotProduct(normalUnitVector, negativeRayDirection) - negativeRayDirection;
            var rayTraceResult = TraceRay(intersectedPoint, reflectDirection, iterationsLeft);
            
            if (rayTraceResult.Success)
            {
                var lenghtToIntersect = (intersectedPoint - rayTraceResult.IntersectedPoint).Lenght;
                return rayTraceResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
            }
            return VecColor.Empty;
        }

        private bool TryGetTheClosestIntersectionWithTheScene(SearchParameters parameters, 
            out ISceneObject intersectedObject, 
            out Vector3 intersectedPoint)
        {
            ISceneObject sceneObject = null;
            var localMaximum = parameters.MaxRayCoefficient;
            foreach (var obj in _sceneObjects)
            {
                foreach (var rayCoef in obj.FindIntersectedRayCoefficients(parameters.Ray))
                {
                    if (IsValidLength(parameters.MinRayCoefficient, localMaximum, rayCoef))
                    {
                        localMaximum = rayCoef;
                        sceneObject = obj;
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
                if(cos <= 0)
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