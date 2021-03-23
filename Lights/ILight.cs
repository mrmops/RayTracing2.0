namespace RayTracing2._0
{
    public interface ILight
    {
        SearchParameters GetSearchParametersForEclipsingObjects(Vector3 crossPoint);

        Vector3 GetDirection(Vector3 crossPoint);
    }
}