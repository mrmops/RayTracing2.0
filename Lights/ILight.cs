using System.Numerics;
using RayTracing2._0.Infostructure;

namespace RayTracing2._0.Lights;

public interface ILight
{
    SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint);

    Vector3 GetDirection(Vector3 point);
}