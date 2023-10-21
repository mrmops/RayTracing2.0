using System;
using System.Linq;
using System.Numerics;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RayTracing2._0.SceneObjects.Materials;

public class TextureMaterial
{
    private Image<Rgba32> _colorTexture;
    private Image<Rgba32> _normalTexture;
    private Image<Rgba32> _materialParametersTexture;

    public TextureMaterial(string colorTexture, string normalTexture, string materialParametersTexture)
    {
        _colorTexture = _loadTexture(colorTexture);
        _normalTexture = _loadTexture(normalTexture);
        _materialParametersTexture = _loadTexture(materialParametersTexture);
    }

    private Image<Rgba32> _loadTexture(string path) => Image.Load<Rgba32>(path);

    public MaterialData GetMaterialData(Vector2 textureCoordinates, Vector3 normal)
    {
        var normalResult = Vector3.Normalize(_getNormal(textureCoordinates) + normal);
        var color = _getColor(textureCoordinates);
        var parameters = _getMaterialParameters(textureCoordinates);

        return new MaterialData(
            color,
            normalResult,
            parameters
        );
    }

    private MaterialParameters _getMaterialParameters(Vector2 textureCoordinates)
    {
        var color = _getImageData(_materialParametersTexture, textureCoordinates);

        return new MaterialParameters(
            _colorToUnitRange(color.R),
            color.G,
            _colorToUnitRange(color.B),
            _colorToUnitRange(color.A)
        );
    }

    private VecColor _getColor(Vector2 textureCoordinates)
    {
        return VecColor.FromRgba(_getImageData(_colorTexture, textureCoordinates));
    }

    private Vector3 _getNormal(Vector2 textureCoordinates)
    {
        var normalData = _getImageData(_normalTexture, textureCoordinates);
        var normal = new Vector3(
            _colorToUnitVectorRange(normalData.R),
            _colorToUnitVectorRange(normalData.G),
            _colorToUnitVectorRange(normalData.B)
        );
        return normal;
    }

    private float _colorToUnitVectorRange(int value)
    {
        return (value - 127.5f) / 127.5f;
    }

    private float _colorToUnitRange(int value)
    {
        return value / 255f;
    }

    private Rgba32 _getImageData(Image<Rgba32> texture, Vector2 textureCoordinates)
    {
        var imageX = (int)(texture.Width * (textureCoordinates.X + 1) / 2f);
        var imageY = (int)(texture.Height * (textureCoordinates.Y + 1) / 2f);
        
        // Console.WriteLine($"{imageX}:{imageY} - {texture.Width}:{texture.Height}");

        return texture[
            _validateRange(imageX, texture.Width),
            _validateRange(imageY, texture.Height)
        ];
    }

    private static int _validateRange(int actual, int originSize)
    {
        return Math.Max(0, Math.Min(actual, originSize));
    }
}