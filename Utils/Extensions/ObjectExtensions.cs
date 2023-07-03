using System;

namespace RayTracing2._0.Utils.Extensions;

public static class ObjectExtensions
{

    public static TR Let<T, TR>(this T obj, Func< T, TR> action) => action(obj);
}