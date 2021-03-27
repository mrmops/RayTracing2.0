using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracing2._0
{
    public class Scene
    {
        private List<ISceneObject> _sceneObjects = new List<ISceneObject>()
        {
            new Sphere(new Vector3(0, -1, 2), 1, VecColor.FromColor(Color.Red), 0.2, 500),
            new Sphere(new Vector3(-2, 0, 4), 1, VecColor.FromColor(Color.Blue), 0.3, 100),
            new Sphere(new Vector3(2, 0, 4), 1, VecColor.FromColor(Color.Green), 1, 500),
            new Sphere(new Vector3(0, -5001, 0), 5000, VecColor.FromColor(Color.Yellow), 0, 100),
        };
        
        public Vector3 CameraPosition  = new Vector3(0, 0, 0);
        public Matrix RotationMatrix = Matrix.CreateStartMatrix();
        private double _lenghtToNearBorder = 1;

        private List<Light> _lights = new List<Light>()
        {
            new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6), 
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.4),
        };

        public Task<Bitmap> GetFrame(Size frameSize)
        {
            var task = new Task<Bitmap>(() =>
            {
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                
                var canvas = new Bitmap(frameSize.Width, frameSize.Height, PixelFormat.Format32bppArgb);
                var canvasHeight = canvas.Height;
                var canvasWidth = canvas.Width;
                var stepToPixel = 1 / (double)canvasHeight;

                // for(var x = -canvasWidth / 2; x <canvasWidth / 2; x++)
                // {
                //     var dx = x + canvasWidth / 2;
                //
                //
                //     for (int y = -canvasHeight / 2 + 1; y < canvasHeight / 2; y++)
                //     {
                //         var viewPortPosition = CanvasToDirectionViewport(x, y, stepToPixel);
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
                        var directionViewPortPosition = RotationMatrix * CanvasToDirectionViewport(x, y, stepToPixel);
                        var result = TraceRay(camera, directionViewPortPosition, 3);
                        if(result.Success)
                        {
                            var dy = canvasHeight / 2 - y;
                            var color = result.ResultColor.ToColor();
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
                var (diffusedLight, primaryColor) = FindDiffusedAndPrimaryLight(intersectedObject, intersectedPoint, normalUnitVector,
                    iterationsLeft);

                var reflectLight =
                    FindReflectLight(intersectedPoint, normalUnitVector, directionVector, iterationsLeft);
                
                var reflectCoef = intersectedObject.ReflectCoef;

                var resultColor = reflectLight * reflectCoef + diffusedLight * (1 - reflectCoef) + primaryColor + GlobalLightningParameters.BackgroundLight;
                
                return new TraceRayResult(true, intersectedObject, intersectedPoint, resultColor);
            }
            
            return TraceRayResult.Fail();
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
                if(cos <= 0 
                   || TryGetTheClosestIntersectionWithTheScene(new SearchParameters(new Ray(pointToObject, direction)), 
                       out var intersectedObject, out var intersectedPoint))
                {
                    continue;
                }
                //var traceResult = TraceRay(pointToObject, direction, iterationsLeft, maxRayCoefficient: 1);
                 VecColor lightColor;
                // if (traceResult.Success)
                // {
                //     lightColor = traceResult.ResultColor;
                // }
                // else
                // {
                //     
                //     
                // }
                lightColor = light.Color * light.Intensity;
                resultPrimary += lightColor * Math.Pow(cos, sceneObject.SpecularCoef);
                resultDiffused += lightColor * cos;
            }

            return (VecColor.Intersection(resultDiffused, sceneObject.Color), resultPrimary);
        }
    }
}