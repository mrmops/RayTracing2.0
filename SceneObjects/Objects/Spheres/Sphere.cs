using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Materials.Models;

namespace RayTracing2._0.SceneObjects.Objects.Spheres;

public class Sphere : SphereBase, ISceneObject, IRenderObject
{
    public IMaterial Material { get; }

    public Sphere(Vector3 center, double radius, IMaterial material) : base(center, radius)
    {
        Material = material;
    }

    public IEnumerable<(float, IRenderObject)> FindIntersectedRayCoefficients(Ray ray)
    {
        return base.GetIntersectedRayCoefficients(ray).Select(rayCoef => (coef: rayCoef, (IRenderObject)this));
    }

    public MaterialData GetMaterialByIntersection(Vector3 intersectionPoint)
    {
        return MaterialData.FromMaterial(Material, GetNormalUnitVector(intersectionPoint));
    }
}