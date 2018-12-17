using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    internal static class Reflection
    {
        internal static MemberInfo[] PublicInstanceFields(this Type type)
        {
            return type.GetTypeInfo().GetMembers(BindingFlags.Public | BindingFlags.Instance);
        }

        internal static IEnumerable<MemberInfo> OrderByDeclaration(this IEnumerable<MemberInfo> members)
        {
            if (members.Count() == 0) return members;
            Type declaringType = members.First().DeclaringType;
            if (declaringType.GetTypeInfo().StructLayoutAttribute.Value
                == System.Runtime.InteropServices.LayoutKind.Sequential)
            {
                return members.OrderBy(m => m.MetadataToken);
            }
            else if (IsValueTuple(declaringType))
            {
                return members.OrderBy(m => m.Name);
            }
            throw new ArgumentException($"Result type {declaringType.Name} must have sequential StructLayout");
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

        internal static Type ValueType(this MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            else if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            throw new NotSupportedException(member.MemberType.ToString());
        }

        internal static bool IsScalar(this Type type)
        {
            return type == typeof(string)
                || type == typeof(Memory<char>)
                || type == typeof(ReadOnlyMemory<char>)
                || type.GetTypeInfo().IsPrimitive;
        }
    }
}
