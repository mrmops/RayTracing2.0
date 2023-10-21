using System.Numerics;

namespace RayTracing2._0.SceneObjects.Objects.Models;

public record TexturePoint(
    Vector3 Location,
    Vector2 TextureCoordinates
);