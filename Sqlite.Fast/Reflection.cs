using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    internal static class Reflection
    {
        internal static IEnumerable<MemberInfo> FieldsOrderedByDeclaration(this Type type)
        {
            return OrderByDeclaration(type, PublicInstanceFields(type));
        }

        private static MemberInfo[] PublicInstanceFields(Type type)
        {
            return type.GetTypeInfo().GetMembers(BindingFlags.Public | BindingFlags.Instance);
        }

        private static IEnumerable<MemberInfo> OrderByDeclaration(Type declaringType, IEnumerable<MemberInfo> members)
        {
            if (members.Count() == 0) return members;
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

        internal static bool IsNullable(this Type type, out Type innerType)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                innerType = type.GenericTypeArguments[0];
                return true;
            }
            innerType = default;
            return false;
        }

        internal static bool IsScalar(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            return type == typeof(string)
                || type == typeof(Memory<char>)
                || type == typeof(ReadOnlyMemory<char>)
                || typeInfo.IsPrimitive
                || typeInfo.IsEnum
                || type == typeof(Guid)
                || IsNullable(type, out Type innerType) && IsScalar(innerType);
        }
    }
}
