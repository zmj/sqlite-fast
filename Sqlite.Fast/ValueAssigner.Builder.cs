using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate T FromInteger<T>(long value);
    public delegate T FromFloat<T>(double value);
    public delegate T FromText<T>(ReadOnlySpan<char> value);
    internal delegate T FromUtf8Text<T>(ReadOnlySpan<byte> value); // internal until official type
    public delegate T FromBlob<T>(ReadOnlySpan<byte> value);
    public delegate T FromNull<T>();

    internal delegate void FieldSetter<TResult, TField>(ref TResult rec, TField value);

    internal static class ValueAssigner
    {
        internal static IBuilder<TResult> Build<TResult>(MemberInfo member)
        {
            var constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TResult), GetMemberType(member) })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return (IBuilder<TResult>)constructor.Invoke(new[] { member });
        }

        internal static Type GetMemberType(MemberInfo member)
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
        
        internal interface IBuilder<TResult>
        {
            MemberInfo Member { get; }
            IValueAssigner<TResult> Compile(bool withDefaults);
            Builder<TResult, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TResult, TField> : IBuilder<TResult>
        {
            public MemberInfo Member { get; }

            public FromInteger<TField> FromInteger;
            public FromFloat<TField> FromFloat;
            public FromText<TField> FromUtf16Text;
            public FromUtf8Text<TField> FromUtf8Text;
            public FromBlob<TField> FromBlob;
            public FromNull<TField> FromNull;

            public Builder(MemberInfo member)
            {
                Member = member;
            }

            public IValueAssigner<TResult> Compile(bool withDefaults)
            {
                if (withDefaults)
                {
                    FromInteger = FromInteger ?? DefaultConverters.GetIntegerConverter<TField>();
                    FromFloat = FromFloat ?? DefaultConverters.GetFloatConverter<TField>();
                    FromUtf16Text = FromUtf16Text ?? DefaultConverters.GetUtf16TextConverter<TField>();
                    FromUtf8Text = FromUtf8Text ?? DefaultConverters.GetUtf8TextConverter<TField>();
                    FromBlob = FromBlob ?? DefaultConverters.GetBlobConverter<TField>();
                    FromNull = FromNull ?? DefaultConverters.GetNullConverter<TField>();
                }
                return new ValueAssigner<TResult, TField>(
                    Member.Name,
                    CompileSetter(Member),
                    FromInteger,
                    FromFloat,
                    FromUtf16Text,
                    FromUtf8Text,
                    FromBlob,
                    FromNull);
            }

            private static FieldSetter<TResult, TField> CompileSetter(MemberInfo memberInfo)
            {
                var result = Expression.Parameter(typeof(TResult).MakeByRefType());
                Expression member;
                if (memberInfo is FieldInfo fieldInfo)
                {
                    member = Expression.Field(result, fieldInfo);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    member = Expression.Property(result, propertyInfo);
                }
                else
                {
                    throw new ArgumentException($"Cannot set value of {memberInfo.DeclaringType.Name}.{memberInfo.Name}");
                }
                var value = Expression.Parameter(typeof(TField));
                var assignment = Expression.Assign(member, value);
                var lambda = Expression.Lambda<FieldSetter<TResult, TField>>(assignment, result, value);
                return lambda.Compile();
            }

            public Builder<TResult, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TResult, TCallerField> builder)
                {
                    return builder;
                }
                throw new InvalidOperationException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }
    }
}
