using RayTracing2._0.SceneObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using RayTracing2._0.Infostructure;
using RayTracing2._0.Lights;
using RayTracing2._0.SceneObjects.Materials;

namespace RayTracing2._0
{
    public class Scene
    {
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

            new Cube(new Glass(VecColor.FromColor(Color.SlateGray /*Color.FromArgb(255, 110, 0 , 150)*/), 100, 0.05), 1, new Vector3(0,0.5f, 0)),

            new Prism(2, new CustomMaterial(VecColor.FromColor(Color.Blue), 0, 100, 0, 1)),

            new Sphere(new Vector3(2, 0, 4), 1,
                new CustomMaterial(VecColor.FromColor(Color.Green), 1, 500, 0, 1)),

            new Sphere(new Vector3(0, -5001, 0), 5000,
                new CustomMaterial(VecColor.FromColor(Color.Yellow), 0, 100, 0, 1)),
        };

        public Vector3 CameraPosition = new Vector3(0, 0, 0);
        public Matrix4x4 RotationMatrix = Matrix4x4.Identity;
        private float _lenghtToNearBorder = 1;

        private List<Light> _lights = new List<Light>()
        {
            new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6),
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.4),
        };

        public Task<List<KeyValuePair<Point, VecColor>>> GetFrame(Size frameSize)
        {
            var task = new Task<List<KeyValuePair<Point, VecColor>>>(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var rotationMatrix = RotationMatrix;
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);

                var canvasHeight = frameSize.Height;
                var canvasWidth = frameSize.Width;
                var resultColors = new ConcurrentBag<KeyValuePair<Point, VecColor>>();

                var stepToPixel = (1f / canvasHeight);


                ConcurrentBag<int> unCalculatedPixels = new ConcurrentBag<int>(Enumerable.Range(0, canvasWidth * canvasHeight));

                Parallel.For(0, 16, _ =>
                {
                    while (true)
                    {
                        var success = unCalculatedPixels.TryTake(out int pixelIndex);

                        if (!success)
                        {
                            return;
                        }

                        var (dx, dy) = Math.DivRem(pixelIndex, canvasHeight);

                        var x = dx - canvasWidth / 2;
                        var y = -(dy - canvasHeight / 2);

                        var directionViewPortPosition =
                            Vector3.Transform(CanvasToDirectionViewport(x, y, stepToPixel),
                                rotationMatrix
                            );

                        var result = TraceRay(camera, directionViewPortPosition, 4);

                        if (result != null)
                        {
                            resultColors.Add(
                                new KeyValuePair<Point, VecColor>(
                                    new Point(dx, dy),
                                    result.ResultColor)
                            );
                        }
                    }
                });

                stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
                return resultColors.ToList();
            });
            task.Start();
            return task;
        }


        private Vector3 CanvasToDirectionViewport(int x, int y, float stepToPixel)
        {
            return new Vector3(
                x * stepToPixel,
                y * stepToPixel,
                _lenghtToNearBorder
            );
        }

        private TraceRayResult? TraceRay(
            Vector3 rayStart,
            Vector3 directionVector,
            int iterationsLeft,
            float minRayCoefficient = 0.001f,
            float maxRayCoefficient = float.PositiveInfinity
        )
        {
            if (iterationsLeft == 0)
            {
                return null;
            }

            iterationsLeft--;

            var ray = new Ray(rayStart, directionVector);
            var searchParameters = new SearchParameters(ray, minRayCoefficient, maxRayCoefficient);

            var intersectionResult = TryGetTheClosestIntersectionWithTheScene(searchParameters);

            if (intersectionResult == null)
            {
                return null;
            }

            var resultColor = CalculateResultColor(directionVector, iterationsLeft, intersectionResult.Value);

            return new TraceRayResult(
                intersectionResult.Value,
                resultColor
            );
        }

        private VecColor CalculateResultColor(
            Vector3 directionVector,
            int iterationsLeft,
            IntersectionResult intersectionResult
        )
        {
            var (diffusedLight, primaryLight) = FindDiffusedAndPrimaryLight(
                intersectionResult.IntersectedObject,
                intersectionResult.IntersectedPoint,
                intersectionResult.NormalVector,
                iterationsLeft
            );

            var reflectCoef = intersectionResult.IntersectedObject.Material.ReflectCoef;

            var reflectedLight = reflectCoef != 0
                ? FindReflectLight(
                    intersectionResult.IntersectedPoint,
                    intersectionResult.NormalVector,
                    directionVector,
                    iterationsLeft
                ) * reflectCoef
                : VecColor.Empty;

            var refractedLight = intersectionResult.IntersectedObject.Material.AbsorptionCoefficient < 1
                ? FindRefractedLight(
                    intersectionResult.IntersectedObject,
                    intersectionResult.IntersectedPoint,
                    intersectionResult.NormalVector,
                    Vector3.Normalize(directionVector),
                    iterationsLeft
                )
                : VecColor.Empty;


            var absorptionCoefficient = intersectionResult.IntersectedObject.Material.AbsorptionCoefficient;

            var resultColor = reflectedLight
                              + ((diffusedLight + primaryLight + GlobalLightningParameters.BackgroundLight) *
                                 absorptionCoefficient
                                 + refractedLight * (1 - absorptionCoefficient)) * (1 - reflectCoef);
            return resultColor;
        }

        private VecColor FindRefractedLight(
            ISceneObject sceneObject,
            Vector3 intersectedPoint,
            Vector3 normalUnitVector,
            Vector3 rayDirectionVector,
            int iterationsLeft)
        {
            var localDirection = rayDirectionVector;
            var refractiveCoef = sceneObject.Material.RefractiveCoef;
            var dotProduct = Vector3.Dot(localDirection, normalUnitVector);

            if (dotProduct > 0)
            {
                refractiveCoef = 1.0 / refractiveCoef;
            }

            // TODO i don't know what the parameters, find logical names

            var par = (Math.Sqrt(
                           (refractiveCoef * refractiveCoef - 1)
                           / dotProduct * dotProduct + 1)
                       - 1) * dotProduct;

            var vector3 = (float)par * normalUnitVector;

            var refractiveDirection = localDirection + vector3;

            var traceRayResult = TraceRay(intersectedPoint, refractiveDirection, iterationsLeft);
            var lenghtToIntersectPoint = traceRayResult != null
                ? (intersectedPoint - traceRayResult.IntersectionResult.IntersectedPoint).Length()
                : 0;

            return VecColor.Intersection(traceRayResult?.ResultColor ?? VecColor.FromColor(Color.SkyBlue),
                       sceneObject.Material.Color)
                   * Math.Pow(Math.E, -1 * sceneObject.Material.AbsorptionCoefficient * lenghtToIntersectPoint);
        }

        private VecColor FindReflectLight(
            Vector3 intersectedPoint,
            Vector3 normalUnitVector,
            Vector3 rayDirectionVector,
            int iterationsLeft
        )
        {
            var negativeRayDirection = -1 * rayDirectionVector;
            var reflectDirection = 2 * normalUnitVector * Vector3.Dot(normalUnitVector, negativeRayDirection) -
                                   negativeRayDirection;
            var rayTraceResult = TraceRay(intersectedPoint, reflectDirection, iterationsLeft);

            if (rayTraceResult != null)
            {
                var lenghtToIntersect =
                    (intersectedPoint - rayTraceResult.IntersectionResult.IntersectedPoint).Length();
                return rayTraceResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
            }

            return rayTraceResult?.ResultColor ?? VecColor.FromColor(Color.SkyBlue) /** Math.Pow(Math.E, -1)*/;
        }

        private IntersectionResult? TryGetTheClosestIntersectionWithTheScene(SearchParameters parameters)
        {
            IntersectionResult? result = null;
            var localMaximum = parameters.MaxRayCoefficient;

            foreach (var obj in _sceneObjects)
            {
                foreach (var (intersectionRayCoef, normal) in obj.FindIntersectedRayCoefficients(parameters.Ray))
                {
                    if (IsValidLength(parameters.MinRayCoefficient, localMaximum, intersectionRayCoef))
                    {
                        localMaximum = intersectionRayCoef;
                        result = new IntersectionResult(obj, parameters.Ray.GetPointFromCoefficient(localMaximum),
                            Vector3.Normalize(normal));
                    }
                }
            }

            return result;
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
                var unitDirection = Vector3.Normalize(direction);

                var cos = Vector3.Dot(normalUnitVectorToPoint, unitDirection);

                if (cos <= 0)
                {
                    continue;
                }

                var traceResult = TraceRay(pointToObject, direction, iterationsLeft, maxRayCoefficient: 1);
                VecColor lightColor;

                if (traceResult != null)
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