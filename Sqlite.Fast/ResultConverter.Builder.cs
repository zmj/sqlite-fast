using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Sqlite.Fast
{
    public sealed partial class ResultConverter<TResult>
    {
        /// <summary>
        /// ResultConverter.Builder constructs a custom ResultConverter.
        /// </summary>
        public sealed class Builder
        {
            private readonly List<ValueAssigner.IBuilder<TResult>> _assignerBuilders =
                new List<ValueAssigner.IBuilder<TResult>>();
            private readonly bool _withDefaults;

            /// <summary>
            /// Creates a builder for a custom ResultConverter.
            /// Call builder.With(...) to define member conversions, then builder.Compile().
            /// </summary>
            /// <param name="withDefaultConversions">If true, member conversions will fall back to default conversion when no custom conversion is defined.</param>
            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _assignerBuilders = typeof(TResult)
                    .FieldsOrderedByDeclaration()
                    .Where(CanSetValue)
                    .Select(m => ValueAssigner.Build<TResult>(m))
                    .ToList();
            }

            private ValueAssigner.IBuilder<TResult> GetOrAdd(MemberInfo member)
            {
                var builder = _assignerBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueAssigner.Build<TResult>(member);
                    _assignerBuilders.Add(builder);
                }
                return builder;
            }

            private ValueAssigner.Builder<TResult, TField> GetOrAdd<TField>(
                Expression<Func<TResult, TField>> propertyOrField)
            {
                MemberInfo member = GetSettableMember(propertyOrField);
                return GetOrAdd(member).AsConcrete<TField>();
            }

            /// <summary>
            /// Compiles the custom conversions to a ResultConverter instance.
            /// </summary>
            public ResultConverter<TResult> Compile()
            {
                return new ResultConverter<TResult>(
                    CompileInitializer(),
                    _assignerBuilders.Select(b => b.Compile(_withDefaults)));
            }

            private Func<TResult>? CompileInitializer()
            {
                if (typeof(TResult).IsValueType)
                {
                    return null;
                }
                ConstructorInfo? defaultConstructor =
                    typeof(TResult).GetConstructor(new Type[] { });
                if (defaultConstructor == null)
                {
                    // throw this at compile?
                    // depends if (out notNullObject) is allowed
                    return () => throw new ArgumentException(
                        $"{typeof(TResult)} does not have a default constructor");
                }
                return Expression.Lambda<Func<TResult>>(
                    Expression.New(defaultConstructor))
                    .Compile();
            }

            /// <summary>
            /// Defines a conversion from a SQLite integer to a member value.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TResult, TField>> propertyOrField,
                Func<long, TField> fromInteger)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                fromInteger.ThrowIfNull(nameof(fromInteger));
                GetOrAdd(propertyOrField).FromInteger = fromInteger;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a SQLite float to a member value.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TResult, TField>> propertyOrField,
                Func<double, TField> fromFloat)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                fromFloat.ThrowIfNull(nameof(fromFloat));
                GetOrAdd(propertyOrField).FromFloat = fromFloat;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a UTF-16 string to a member value.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="fromText">Deserializes a value from a source ReadOnlySpan&lt;char&gt;</param>
            public Builder With<TField>(
                Expression<Func<TResult, TField>> propertyOrField, 
                FromSpan<char, TField> fromText)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                fromText.ThrowIfNull(nameof(fromText));
                GetOrAdd(propertyOrField).FromUtf16Text = fromText;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a byte sequence to a member value.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="fromBytes">Deserializes a value from a source ReadOnlySpan&lt;byte&gt;</param>
            public Builder With<TField>(
                Expression<Func<TResult, TField>> propertyOrField,
                FromSpan<byte, TField> fromBytes)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                fromBytes.ThrowIfNull(nameof(fromBytes));
                GetOrAdd(propertyOrField).FromBlob = fromBytes;
                return this;
            }

            /// <summary>
            /// Defines a conversion from SQLite null to a member value.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TResult, TField>> propertyOrField,
                Func<TField> fromNull)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                fromNull.ThrowIfNull(nameof(fromNull));
                GetOrAdd(propertyOrField).FromNull = fromNull;
                return this;
            }

            /// <summary>
            /// Removes all defined conversions for a member.
            /// </summary>
            public Builder Ignore<TField>(Expression<Func<TResult, TField>> propertyOrField)
            {
                propertyOrField.ThrowIfNull(nameof(propertyOrField));
                MemberInfo member = GetSettableMember(propertyOrField);
                _assignerBuilders.RemoveAll(builder => builder.Member == member);
                return this;
            }

            private static MemberInfo GetSettableMember<TField>(
                Expression<Func<TResult, TField>> propertyOrField)
            {
                if (propertyOrField.Body is MemberExpression memberExpression &&
                    CanSetValue(memberExpression.Member))
                {
                    return memberExpression.Member;
                }
                throw new ArgumentException($"Expression is not settable field or property of {typeof(TResult).Name}");
            }

            private static bool CanSetValue(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanWrite;
                }
                else if (member is FieldInfo field)
                {
                    return !field.IsInitOnly;
                }
                return false;
            }
        }
    }
}
