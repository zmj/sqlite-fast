using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    internal static class Reflection
    {
        public static IEnumerable<MemberInfo> GetOrderedMembers(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.StructLayoutAttribute.Value
                == System.Runtime.InteropServices.LayoutKind.Sequential)
            {
                return typeInfo
                     .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(m => m.MetadataToken);
            }
            else if (IsValueTuple(type))
            {
                return typeInfo
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(m => m.Name);
            }
            throw new ArgumentException($"Result type {typeInfo.Name} must have sequential StructLayout");
        }

        private static bool IsValueTuple(Type type)
        {
            Type genericType;
            if (type.IsConstructedGenericType) genericType = type.GetGenericTypeDefinition();
            else return false;
            return genericType == typeof(ValueTuple)
                || genericType == typeof(ValueTuple<>)
                || genericType == typeof(ValueTuple<,>)
                || genericType == typeof(ValueTuple<,,>)
                || genericType == typeof(ValueTuple<,,,>)
                || genericType == typeof(ValueTuple<,,,,>)
                || genericType == typeof(ValueTuple<,,,,,>)
                || genericType == typeof(ValueTuple<,,,,,,>)
                || genericType == typeof(ValueTuple<,,,,,,,>);
        }
    }
}
