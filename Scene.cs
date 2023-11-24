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
using RayTracing2._0.SceneObjects.Materials.Models;
using RayTracing2._0.SceneObjects.Objects;
using RayTracing2._0.SceneObjects.Objects.Spheres;
using RayTracing2._0.Utils.Extensions;

namespace RayTracing2._0;

public class Scene {
    private readonly IReadOnlyCollection<ISceneObject> _sceneObjects = new List<ISceneObject> {
        new Sphere(
            new Vector3(0, -1, 2),
            1,
            new CustomMaterial(
                VecColor.FromColor(Color.Blue),
                0.3,
                500,
                2.5,
                0.5
            )
        ),
        new Lens(
            new Glass(
                VecColor.FromColor(Color.Green),
                100,
                0
            ),
            new Vector3(0, 0, 7),
            1
        ),

        new Sphere(
            new Vector3(-2, 0, 4),
            1,
            new Glass(
                VecColor.FromColor(Color.Red),
                50,
                0.02
            )
        ),

        new Cube(
            new TextureMaterial(
                "Textures/colors.tga",
                "Textures/normals.tga",
                "Textures/parameters2.tga"
            ),
            // new Glass(
            //     VecColor.FromColor(Color.SlateGray),
            //     100,
            //     0.05
            // ),
            1,
            new Vector3(0f, 0.5f, 0)
        ),

        // new Prism(
        //     2,
        //     new CustomMaterial(
        //         VecColor.FromColor(Color.Blue),
        //         0,
        //         100,
        //         0,
        //         1
        //     )
        // ),

        new Sphere(
            new Vector3(2, 0, 4),
            1,
            new CustomMaterial(
                VecColor.FromColor(Color.Green),
                1,
                500,
                0,
                1
            )
        ),

        new Sphere(
            new Vector3(0, -5001, 0),
            5000,
            new CustomMaterial(
                VecColor.FromColor(Color.Yellow),
                0,
                100,
                0,
                1
            )
        ),
    };

    public Vector3 CameraPosition = new(0, 0, 0);
    public Matrix4x4 RotationMatrix = Matrix4x4.Identity;
    private readonly float _lenghtToNearBorder = 1;

    private readonly IReadOnlyCollection<Light> _lights = new List<Light> {
        new PointLight(new Vector3(1, 2, 6), VecColor.FromColor(Color.White), 0.6),
        new DirectionLight(new Vector3(0, 4, -7), VecColor.FromColor(Color.Red), 0.2),
        new DirectionLight(new Vector3(9, 4, -7), VecColor.FromColor(Color.White), 0.6),
        new DirectionLight(new Vector3(-9, 4, -7), VecColor.FromColor(Color.White), 0.6),
    };

    public async IAsyncEnumerable<List<KeyValuePair<Point, VecColor?>>>  StartRenderFrames(Size frameSize) {
        while (true) {
            yield return await _getFrame(frameSize);
        }
    }

    private Task<List<KeyValuePair<Point, VecColor?>>> _getFrame(Size frameSize) {
        var task = new Task<List<KeyValuePair<Point, VecColor?>>>(
            () => {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var rotationMatrix = RotationMatrix;
                var camera = new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);

                var canvasHeight = frameSize.Height;
                var canvasWidth = frameSize.Width;
                var resultColors = new ConcurrentBag<KeyValuePair<Point, VecColor?>>();

                var stepToPixel = (1f / canvasHeight);


                ConcurrentBag<int> unCalculatedPixels =
                    new ConcurrentBag<int>(Enumerable.Range(0, canvasWidth * canvasHeight));

                Parallel.For(
                    0, Environment.ProcessorCount, _ => {
                        while (true) {
                            var success = unCalculatedPixels.TryTake(out var pixelIndex);

                            if (!success) {
                                return;
                            }

                            var (px, py) = Math.DivRem(pixelIndex, canvasHeight);

                            var x = px - canvasWidth / 2;
                            var y = -(py - canvasHeight / 2);

                            var pixelDirection = _calculateDirectionFromViewportCoordinates(x, y, stepToPixel);
                            var pixelDirectionRotated =
                                Vector3.Transform(
                                    pixelDirection,
                                    rotationMatrix
                                );

                            var result = TraceRay(new(camera, pixelDirectionRotated), 6);

                            if (result != null) {
                                resultColors.Add(
                                    new KeyValuePair<Point, VecColor?>(
                                        new Point(px, py),
                                        result.ResultColor
                                    )
                                );
                            }
                        }
                    }
                );

                stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
                return resultColors.ToList();
            }
        );
        task.Start();
        return task;
    }


    private Vector3 _calculateDirectionFromViewportCoordinates(int x, int y, float stepToPixel) =>
        new(
            x * stepToPixel,
            y * stepToPixel,
            _lenghtToNearBorder
        );

    private TraceRayResult? TraceRay(
        Ray ray,
        int iterationsLeft,
        float minRayCoefficient = 0.001f,
        float maxRayCoefficient = float.PositiveInfinity
    ) {
        if (iterationsLeft == 0) {
            return null;
        }

        iterationsLeft--;

        var searchParameters = new SearchParameters(ray, minRayCoefficient, maxRayCoefficient);

        var intersectionResult = TryGetTheClosestIntersectionWithTheScene(searchParameters);

        if (intersectionResult == null) {
            return null;
        }

        var resultColor = CalculateResultColor(ray.Direction, iterationsLeft, intersectionResult.Value);

        return new TraceRayResult(
            intersectionResult.Value,
            resultColor
        );
    }

    private VecColor CalculateResultColor(
        Vector3 direction,
        int iterationsLeft,
        IntersectionResult intersectionResult
    ) {
        if (Math.Abs(intersectionResult.MaterialData.Parameters.ReflectCoefficient - 1) < 0.00000000000000000001) {
            return FindReflectLight(
                intersectionResult.IntersectedPoint,
                intersectionResult.MaterialData.Normal,
                direction,
                iterationsLeft
            );
        }

        var reflection = intersectionResult.MaterialData.Parameters.ReflectCoefficient;

        var reflectedLight = reflection > 0
            ? FindReflectLight(
                intersectionResult.IntersectedPoint,
                intersectionResult.MaterialData.Normal,
                direction,
                iterationsLeft
            ) * reflection
            : VecColor.Empty;

        var absorptionCoefficient = intersectionResult.MaterialData.Parameters.AbsorptionCoefficient;

        var refractedLight = intersectionResult.MaterialData.Parameters.AbsorptionCoefficient < 1
            ? FindRefractedLight(
                intersectionResult.MaterialData,
                intersectionResult.IntersectedPoint,
                Vector3.Normalize(direction),
                iterationsLeft
            ) * (1 - absorptionCoefficient)
            : VecColor.Empty;

        var (diffusedLight, primaryLight) = absorptionCoefficient > 0
            ? FindDiffusedAndPrimaryLight(
                intersectionResult.MaterialData,
                intersectionResult.IntersectedPoint,
                intersectionResult.MaterialData.Normal,
                iterationsLeft
            )
            : (VecColor.Empty, VecColor.Empty);


        var directLight = diffusedLight + primaryLight + GlobalLightningParameters.BackgroundLight;

        var calculateResultColor = reflectedLight
                                   + (directLight * absorptionCoefficient + refractedLight) * (1 - reflection);
        
        return calculateResultColor;
    }

    private VecColor FindRefractedLight(
        MaterialData materialData,
        Vector3 intersectedPoint,
        Vector3 rayDirection,
        int iterationsLeft
    ) {
        var refraction = materialData.Parameters.RefractiveCoefficient;
        var dotProduct = Vector3.Dot(rayDirection, materialData.Normal);

        if (dotProduct > 0) {
            refraction = 1.0 / refraction;
        }

        var par = (Math.Sqrt(
                       (refraction * refraction - 1)
                       / dotProduct * dotProduct + 1
                   )
                   - 1) * dotProduct;

        var vector3 = (float)par * materialData.Normal;

        var refractiveDirection = rayDirection + vector3;

        var traceRayResult = TraceRay(new(intersectedPoint, refractiveDirection), iterationsLeft);

        var lenghtToIntersectPoint = traceRayResult != null
            ? (intersectedPoint - traceRayResult.IntersectionResult.IntersectedPoint).Length()
            : 0;

        return VecColor.Intersection(
                   traceRayResult?.ResultColor ?? VecColor.FromColor(Color.SkyBlue),
                   materialData.Color
               )
               * Math.Pow(Math.E, -1 * materialData.Parameters.AbsorptionCoefficient * lenghtToIntersectPoint);
    }

    private VecColor FindReflectLight(
        Vector3 intersectedPoint,
        Vector3 normalUnitVector,
        Vector3 rayDirectionVector,
        int iterationsLeft
    ) {
        var negativeRayDirection = -1 * rayDirectionVector;
        var reflectDirection = 2 * normalUnitVector * Vector3.Dot(normalUnitVector, negativeRayDirection) -
                               negativeRayDirection;
        var rayTraceResult = TraceRay(new(intersectedPoint, reflectDirection), iterationsLeft);

        if (rayTraceResult != null) {
            var lenghtToIntersect =
                (intersectedPoint - rayTraceResult.IntersectionResult.IntersectedPoint).Length();
            return rayTraceResult.ResultColor * Math.Pow(Math.E, -0.01 * lenghtToIntersect);
        }

        return rayTraceResult?.ResultColor ?? VecColor.FromColor(Color.SkyBlue);
    }

    private IntersectionResult? TryGetTheClosestIntersectionWithTheScene(SearchParameters parameters) {
        (IRenderObject, float)? result = null;
        var localMaximum = parameters.MaxRayCoefficient;

        foreach (var obj in _sceneObjects) {
            foreach (var (intersectionRayLength, renderObject) in obj.FindIntersectedRayCoefficients(parameters.Ray)) {
                if (IsValidLength(parameters.MinRayLengthCoefficient, localMaximum, intersectionRayLength)) {
                    localMaximum = intersectionRayLength;
                    result = new(renderObject, localMaximum);
                }
            }
        }

        return result?.Let(
            value => {
                var renderObject = value.Item1;
                var intersectionPoint = parameters.Ray.GetPointFromCoefficient(value.Item2);
                var materialData = renderObject.GetMaterialByIntersection(intersectionPoint);
                return new IntersectionResult(intersectionPoint, materialData);
            }
        );
    }

    private static bool IsValidLength(double tMin, double tMax, double t) {
        return t > tMin && t < tMax;
    }

    private (VecColor, VecColor) FindDiffusedAndPrimaryLight(
        MaterialData materialData,
        Vector3 pointToObject,
        Vector3 normalUnitVectorToPoint,
        int iterationsLeft
    ) {
        var resultDiffused = VecColor.Empty;
        var resultPrimary = VecColor.Empty;

        foreach (var light in _lights) {
            var direction = light.GetDirection(pointToObject);
            var unitDirection = Vector3.Normalize(direction);

            var cos = Vector3.Dot(normalUnitVectorToPoint, unitDirection);

            if (cos <= 0) {
                continue;
            }

            var traceResult = TraceRay(new(pointToObject, direction), iterationsLeft, maxRayCoefficient: 1);
            VecColor lightColor;

            if (traceResult != null) {
                lightColor = traceResult.ResultColor;
            }
            else {
                lightColor = light.Color * light.Intensity;
                resultPrimary += lightColor * Math.Pow(cos, materialData.Parameters.SpecularCoefficient);
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
        return (VecColor.Intersection(resultDiffused, materialData.Color), resultPrimary);
    }
}
