using System.Drawing;
using System.Numerics;

namespace RayTracing2._0
{
    public struct Intersectionresult
    {
        public ISceneObject IntersectedObject;
        public Vector3 IntersectedPoint;
        public Vector3 NormalVector;

        public Intersectionresult(ISceneObject intersectedObject, Vector3 intersectedPoint, Vector3 normalVector)
        {
            IntersectedObject = intersectedObject;
            IntersectedPoint = intersectedPoint;
            NormalVector = normalVector;
        }
    }
}