using RayTracing2._0.Material;
using RayTracing2._0.SceneObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace RayTracing2._0 {
    public class Scene {
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
        public Matrix4x4 RotationMatrix = Matrix4x4.Identity;
        private float _lenghtToNearBorder = 1;

        private List<Light> _lights = new List<Light>()
        {
            new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6),
            new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.White), 0.4),
        };

        public Task<VecColor[,]> GetFrame(Size frameSize, int fragmentsCount) {
            var task = new Task<VecColor[,]>(() => {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);

                var canvasHeight = frameSize.Height;
                var canvasWidth = frameSize.Width;
                VecColor[,] canvasColors = new VecColor[canvasWidth, canvasHeight];

                var stepToPixel = 1 / (float)canvasHeight;

                var columnsPerFragment = canvasWidth / fragmentsCount + 1;

                ConcurrentQueue<int> fragments = new ConcurrentQueue<int>(Enumerable.Range(0, fragmentsCount));

                Parallel.For(0, Math.Min(fragmentsCount, 256), _ => {
                    while(true) {
                        var success = fragments.TryDequeue(out int fragmentIndex);

                        if(!success) {
                            return;
                        }

                        var fragmentShift = fragmentIndex * columnsPerFragment;
                        for(var localX = 0; localX < columnsPerFragment; localX++) {
                            var dx = localX + fragmentShift;
                            if(dx >= canvasWidth)
                                return;
                            var x = dx - canvasWidth / 2;
                            var halfCanvasHeight = canvasHeight / 2;
                            for(var y = -halfCanvasHeight + 1; y < halfCanvasHeight; y++) {
                                var directionViewPortPosition =
                                    Vector3.Transform(CanvasToDirectionViewport(x, y, stepToPixel), RotationMatrix);
                                var result = TraceRay(camera, directionViewPortPosition, 5);
                                var dy = halfCanvasHeight - y;
                                var color = result?.ResultColor ?? VecColor.Empty;
                                canvasColors[dx, dy] = color;
                            }
                        }

                    }
                });

                stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
                return canvasColors;
            });
            task.Start();
            return task;
        }


        private Vector3 CanvasToDirectionViewport(int x, int y, float stepToPixel) {
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
            float minRayCoefficient = 0.0000001f,
            float maxRayCoefficient = float.PositiveInfinity
            ) {

            if(iterationsLeft == 0) {
                return null;
            }

            iterationsLeft--;

            var ray = new Ray(rayStart, directionVector);
            var searchParameters = new SearchParameters(ray, minRayCoefficient, maxRayCoefficient);

            var intersectionResult = TryGetTheClosestIntersectionWithTheScene(searchParameters);

            if(intersectionResult == null) {
                return null;
            }
            var intersectionResult2 = (Intersectionresult)intersectionResult;
            var resultColor = CalculateResultColor(directionVector, iterationsLeft, intersectionResult2);

            return new TraceRayResult(
                intersectionResult2.IntersectedObject,
                intersectionResult2.IntersectedPoint,
                resultColor
            );
        }

        private VecColor CalculateResultColor(
            Vector3 directionVector,
            int iterationsLeft,
            Intersectionresult intersectionResult
        ) {
            var (diffusedLight, primaryLight) = FindDiffusedAndPrimaryLight(
                            intersectionResult.IntersectedObject,
                            intersectionResult.IntersectedPoint,
                            intersectionResult.NormalVector,
                            iterationsLeft
                        );

            var reflectLight = intersectionResult.IntersectedObject.Material.ReflectCoef != 0
                ? FindReflectLight(
                    intersectionResult.IntersectedPoint,
                    intersectionResult.NormalVector,
                    directionVector,
                    iterationsLeft
                )
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

            var reflectCoef = intersectionResult.IntersectedObject.Material.ReflectCoef;

            var absorptionCoefficient = intersectionResult.IntersectedObject.Material.AbsorptionCoefficient;

            var resultColor = reflectLight * reflectCoef
                                + ((diffusedLight
                                    + primaryLight
                                    + GlobalLightningParameters.BackgroundLight) * (absorptionCoefficient)
                                    + refractedLight * (1 - absorptionCoefficient)) * (1 - reflectCoef);
            return resultColor;
        }

        private VecColor FindRefractedLight(ISceneObject sceneObject, Vector3 intersectedPoint,
            Vector3 normalUnitVector, Vector3 rayDirectionVector, int iterationsLeft) {
            var localDirection = rayDirectionVector;
            var refractiveCoef = sceneObject.Material.RefractiveCoef;
            var dotProduct = Vector3.Dot(localDirection, normalUnitVector);
            Vector3 refractiveDirection;
            if(dotProduct > 0) {
                refractiveCoef = 1 / refractiveCoef;

            }

            // TODO i don't know what the parameters, find logical names

            var par = (Math.Sqrt(
                                       (refractiveCoef * refractiveCoef - 1)
                                       / dotProduct * dotProduct + 1)
                                   - 1) * dotProduct;

            var vector3 = Vector3.Multiply((float)par, normalUnitVector);

            refractiveDirection = localDirection + vector3;

            var traceRayResult = TraceRay(intersectedPoint, refractiveDirection, iterationsLeft);
            var lenghtToIntersectPoint = traceRayResult != null
                ? Vector3.Distance(intersectedPoint, traceRayResult.IntersectedPoint)
                : 0;
            return VecColor.Intersection(traceRayResult?.ResultColor ?? VecColor.Empty, sceneObject.Material.Color)
                   * Math.Pow(Math.E, -1 * sceneObject.Material.AbsorptionCoefficient * lenghtToIntersectPoint);
        }

        private VecColor FindReflectLight(Vector3 intersectedPoint, Vector3 normalUnitVector,
            Vector3 rayDirectionVector, int iterationsLeft) {
            var negativeRayDirection = -1 * rayDirectionVector;
            var reflectDirection = 2 * normalUnitVector * Vector3.Dot(normalUnitVector, negativeRayDirection) -
                                   negativeRayDirection;
            var rayTraceResult = TraceRay(intersectedPoint, reflectDirection, iterationsLeft);

            if(rayTraceResult != null) {
                var lenghtToIntersect = Vector3.Distance(intersectedPoint, rayTraceResult.IntersectedPoint);
                return rayTraceResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
            }

            return rayTraceResult?.ResultColor ?? VecColor.Empty/** Math.Pow(Math.E, -1)*/;
        }

        private Intersectionresult? TryGetTheClosestIntersectionWithTheScene(SearchParameters parameters) {
            Intersectionresult? result = null;
            var localMaximum = parameters.MaxRayCoefficient;

            foreach(var obj in _sceneObjects) {
                foreach(var rayCoef in obj.FindIntersectedRayCoefficients(parameters.Ray)) {
                    if(IsValidLength(parameters.MinRayCoefficient, localMaximum, rayCoef.Item1)) {
                        localMaximum = rayCoef.Item1;
                        var normal = rayCoef.Item2;
                        result = new Intersectionresult(obj, parameters.Ray.GetPointFromCoefficient(localMaximum), normal);
                    }
                }
            }

            return result;
        }

        private static bool IsValidLength(double tMin, double tMax, double t) {
            return t > tMin && t < tMax;
        }

        private (VecColor, VecColor) FindDiffusedAndPrimaryLight(ISceneObject sceneObject, Vector3 pointToObject,
            Vector3 normalUnitVectorToPoint, int iterationsLeft) {
            var resultDiffused = VecColor.Empty;
            var resultPrimary = VecColor.Empty;
            foreach(var light in _lights) {
                var direction = light.GetDirection(pointToObject);
                var unitDirection = direction / direction.Length();
                var cos = Vector3.Dot(normalUnitVectorToPoint, unitDirection);
                if(cos <= 0) {
                    continue;
                }

                var traceResult = TraceRay(pointToObject, direction, iterationsLeft, maxRayCoefficient: 1);
                VecColor lightColor;
                if(traceResult != null) {
                    lightColor = traceResult.ResultColor;
                } else {
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

    public class ImageFragment {
        public int FragmentIndex;
        public Color[,] Colors;

        public ImageFragment(int fragmentIndex, Color[,] colors) {
            FragmentIndex = fragmentIndex;
            Colors = colors;
        }
    }
}