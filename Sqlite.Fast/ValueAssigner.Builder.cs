using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Deserializes an instance of T.
    /// </summary>
    /// <typeparam name="TElem">The elemental type of the SQLite value.</typeparam>
    /// <typeparam name="T">The result member type.</typeparam>
    /// <param name="value">A ReadOnlySpan&lt;TElem&gt; view of the SQLite value.</param>
    public delegate T FromSpan<TElem, T>(ReadOnlySpan<TElem> value);

    internal delegate void FieldSetter<TResult, TField>(ref TResult result, TField value);

    internal static class ValueAssigner
    {
        public static IBuilder<TResult> Build<TResult>(MemberInfo member) =>
            (IBuilder<TResult>)BuildInternal<TResult>(member);

        public static Builder<TResult, TResult> Build<TResult>() =>
            (Builder<TResult, TResult>)BuildInternal<TResult>(member: null);

        private static object BuildInternal<TResult>(MemberInfo? member)
        {
            Type valueType = member != null ? member.ValueType() : typeof(TResult);
            ConstructorInfo? constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TResult), valueType })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return constructor!.Invoke(new[] { member });
        }
   
        public interface IBuilder<TResult>
        {
            MemberInfo? Member { get; }
            IValueAssigner<TResult> Compile(bool withDefaults);
            Builder<TResult, TField> AsConcrete<TField>();
        }

        public sealed class Builder<TResult, TField> : IBuilder<TResult>
        {
            public MemberInfo? Member { get; }

            public Func<long, TField>? FromInteger;
            public Func<double, TField>? FromFloat;
            public FromSpan<char, TField>? FromUtf16Text;
            public FromSpan<byte, TField>? FromUtf8Text;
            public FromSpan<byte, TField>? FromBlob;
            public Func<TField>? FromNull;

            public Builder(MemberInfo? member) => Member = member;

            IValueAssigner<TResult> IBuilder<TResult>.Compile(bool withDefaults) => Compile(withDefaults);

            public ValueAssigner<TResult, TField> Compile(bool withDefaults)
            {
                if (withDefaults)
                {
                    FromInteger = FromInteger ?? DefaultConverters.GetIntegerConverter<TField>();
                    FromFloat = FromFloat ?? DefaultConverters.FromFloat<TField>();
                    FromUtf16Text = FromUtf16Text ?? DefaultConverters.FromUtf16Text<TField>();
                    FromUtf8Text = FromUtf8Text ?? DefaultConverters.FromUtf8Text<TField>();
                    FromBlob = FromBlob ?? DefaultConverters.FromBlob<TField>();
                    FromNull = FromNull ?? DefaultConverters.FromNull<TField>();
                }
                return new ValueAssigner<TResult, TField>(
                    Member?.Name,
                    CompileSetter(Member),
                    FromInteger,
                    FromFloat,
                    FromUtf16Text,
                    FromUtf8Text,
                    FromBlob,
                    FromNull);
            }

            private static FieldSetter<TResult, TField> CompileSetter(MemberInfo? memberInfo)
            {
                var result = Expression.Parameter(typeof(TResult).MakeByRefType());
                Expression target;
                if (memberInfo == null)
                {
                    target = result;
                }
                else if (memberInfo is FieldInfo fieldInfo)
                {
                    target = Expression.Field(result, fieldInfo);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    target = Expression.Property(result, propertyInfo);
                }
                else
                {
                    throw new ArgumentException($"Cannot set value of {memberInfo.DeclaringType?.Name}.{memberInfo.Name}");
                }
                var value = Expression.Parameter(typeof(TField));
                var assignment = Expression.Assign(target, value);
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
