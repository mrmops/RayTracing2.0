using System;
using System.Numerics;
using System.Threading;
using RayTracing2._0.Infostructure;
using RayTracing2._0.SceneObjects.Materials;
using RayTracing2._0.SceneObjects.Materials.Models;
using RayTracing2._0.SceneObjects.Objects.Models;

namespace RayTracing2._0.SceneObjects.Objects;

public class Triangle : IRenderObject
{
    private readonly Vector3 _sideAbVector;
    private readonly Vector3 _sideAcVector;

    private readonly Vector2 _textureAbBasisVector;
    private readonly Vector2 _textureAcBasisVector;

    private readonly Matrix2x2 _inverseTextureTransitionMatrix;
    public TexturePoint SideA { get; }
    public TexturePoint SideB { get; }
    public TexturePoint SideC { get; }
    public Vector3 PlaneNormal { get; }
    public TextureMaterial Material { get; }

    public Triangle(
        TexturePoint sideA,
        TexturePoint sideB,
        TexturePoint sideC,
        Vector3 planeNormal,
        TextureMaterial material
    )
    {
        SideA = sideA;
        SideB = sideB;
        SideC = sideC;
        PlaneNormal = planeNormal;
        Material = material;
        _sideAbVector = SideB.Location - SideA.Location;
        _sideAcVector = SideC.Location - SideA.Location;

        _textureAbBasisVector = SideB.TextureCoordinates - SideA.TextureCoordinates;
        _textureAcBasisVector = SideC.TextureCoordinates - SideA.TextureCoordinates;

        // Создаем матрицу перехода от исходного базиса к новому базису
        Matrix2x2 transitionMatrix = new Matrix2x2(
            _textureAbBasisVector.X,
            _textureAbBasisVector.Y,
            _textureAcBasisVector.X,
            _textureAcBasisVector.Y
        );

        // Вычисляем обратную матрицу перехода
        _inverseTextureTransitionMatrix = GetInverseMatrix(transitionMatrix);
    }

    public bool FindIntersectedRayCoefficients(Ray ray, out float t)
    {
        t = 0;
        var pVec = Vector3.Cross(ray.Direction, _sideAcVector);
        var det = Vector3.Dot(_sideAbVector, pVec);
        if (det < 1e-8 && det > -1e-8)
            return false;

        var invDet = 1 / det;
        var tVec = ray.StartPoint - SideA.Location;
        var u = Vector3.Dot(tVec, pVec) * invDet;
        if (u < 0 || u > 1)
            return false;

        var qVec = Vector3.Cross(tVec, _sideAbVector);
        var v = Vector3.Dot(ray.Direction, qVec) * invDet;
        if (v < 0 || u + v > 1)
            return false;

        t = Vector3.Dot(_sideAcVector, qVec) * invDet;
        return true;
    }

    public MaterialData GetMaterialByIntersection(Vector3 intersectionPoint)
    {
        var triangleCoordinates = GetBarycentricCoordinates(intersectionPoint);

        var textureCoordinates = ConvertToNewBasis(triangleCoordinates);

       

        // if (textureCoordinates.X <= -0.9 || textureCoordinates.Y <= -0.9)
        // {
        //     Console.WriteLine($"{triangleCoordinates}, {textureCoordinates}");
        //     
        // }
        //
        // return new MaterialData(
        //     new VecColor(
        //         (int)(textureCoordinates.X * 255),
        //         (int)(textureCoordinates.Y * 255),
        //         0
        //     ),
        //     PlaneNormal,
        //     new MaterialParameters(
        //         0,
        //         1,
        //         0,
        //         1
        //     )
        // );

        return Material.GetMaterialData(textureCoordinates, PlaneNormal);
    }

    Vector2 GetBarycentricCoordinates(Vector3 point)
    {
        // Вычисляем векторы от вершин треугольника к точке P
        Vector3 vectorAp = point - SideA.Location;
        Vector3 vectorBp = point - SideB.Location;
        Vector3 vectorCp = point - SideC.Location;

        // Вычисляем площади треугольников
        float areaAbc = 0.5f * Vector3.Cross(_sideAbVector, _sideAcVector).Length();
        float areaPbc = 0.5f * Vector3.Cross(vectorBp, vectorCp).Length();
        float areaPca = 0.5f * Vector3.Cross(vectorCp, vectorAp).Length();
        // float areaPab = 0.5f * Vector3.Cross(vectorAp, vectorBp).Length();


        // Вычисляем двумерные координаты точки P в базисе треугольника
        float bary1 = areaPbc / areaAbc;
        float bary2 = areaPca / areaAbc;
        // float bary3 = areaPab / areaAbc;

        return new Vector2(bary1, bary2);
    }


    Vector2 ConvertToNewBasis(Vector2 point)
    {
        // Вычисляем вектор от исходного базиса к точке

        // Переводим вектор к новому базису
        Vector2 vectorInNewBasis = _inverseTextureTransitionMatrix * point;

        return vectorInNewBasis;
    }

    static Matrix2x2 GetInverseMatrix(Matrix2x2 matrix)
    {
        float determinant = matrix.A * matrix.D - matrix.B * matrix.C;

        if (determinant != 0)
        {
            float inverseDeterminant = 1.0f / determinant;

            float a = matrix.D * inverseDeterminant;
            float b = -matrix.B * inverseDeterminant;
            float c = -matrix.C * inverseDeterminant;
            float d = matrix.A * inverseDeterminant;

            return new Matrix2x2(a, b, c, d);
        }

        return new Matrix2x2(0, 0, 0, 0);
    }


    // Vector2 GetBarycentricCoordinates(Vector3 point)
    // {
    //     // Находим нормаль плоскости
    //     Vector3 normal = Vector3.Cross(_e1, _e2);
    //
    //     // Находим площадь треугольника
    //     float area = 0.5f * normal.Length();
    //
    //     // Нормализуем нормаль плоскости
    //     Vector3 normalizedNormal = Vector3.Normalize(normal);
    //
    //     // Вычисляем векторные произведения сторон B и вектора OP
    //     Vector3 vectorM = Vector3.Cross(_e2, point);
    //
    //     // Вычисляем векторные произведения сторон A и вектора OP
    //     Vector3 vectorL = Vector3.Cross(_e1, point);
    //
    //     // Вычисляем модули векторов M и L
    //     float modM = vectorM.Length();
    //     float modL = vectorL.Length();
    //
    //     // Вычисляем барицентрические координаты
    //     float alpha = modM / area;
    //     float beta = modL / area;
    //
    //     return new Vector2(alpha, beta);
    // }


    // Vector2 FindLocalPointToTriangle(Vector3 point)
    // {
    //     // Находим скалярное произведение вектора OP и нормали плоскости
    //     Vector3 localPointCoordinates = point - SideA.Location;
    //     float scalar = Vector3.Dot(localPointCoordinates, PlaneNormal);
    //
    //     // Вычисляем проекцию точки на плоскость
    //     Vector3 projectedPoint = point - scalar * PlaneNormal;
    //
    //     // Возвращаем двумерные координаты точки
    //     return new Vector2(projectedPoint.X, projectedPoint.Y);
    // }
}

class Matrix2x2
{
    public float A { get; set; }
    public float B { get; set; }
    public float C { get; set; }
    public float D { get; set; }

    public Matrix2x2(float a, float b, float c, float d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    public static Vector2 operator *(Matrix2x2 m, Vector2 v)
    {
        float x = m.A * v.X + m.B * v.Y;
        float y = m.C * v.X + m.D * v.Y;

        return new Vector2(x, y);
    }
}