using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace Sqlite.Fast
{
#nullable enable
    /// <summary>
    /// ParameterConverter binds an instance of the parameter type to SQLite parameter values.
    /// </summary>
    public sealed partial class ParameterConverter<TParams>
    {
        /// <summary>
        /// ParameterConverter.Builder constructs a custom ParameterConverter.
        /// </summary>
        public sealed class Builder
        {
            private readonly List<ValueBinder.IBuilder<TParams>> _binderBuilders;
            private readonly bool _withDefaults;

            /// <summary>
            /// Creates a builder for a custom ParameterConverter.
            /// Call builder.With(...) to define field conversions, then builder.Compile().
            /// </summary>
            /// <param name="withDefaultConversions">If true, field conversions will fall back to default conversion when no custom conversion can be used.</param>
            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _binderBuilders = typeof(TParams)
                    .FieldsOrderedByDeclaration()
                    .Where(CanGetValue)
                    .Select(m => ValueBinder.Build<TParams>(m))
                    .ToList();
            }

            private ValueBinder.IBuilder<TParams> GetOrAdd(MemberInfo member)
            {
                var builder = _binderBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueBinder.Build<TParams>(member);
                    _binderBuilders.Add(builder);
                }
                return builder;
            }

            private ValueBinder.Builder<TParams, TField> GetOrAdd<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                MemberInfo member = GetGettableMember(propertyOrField);
                return GetOrAdd(member).AsConcrete<TField>();
            }

            /// <summary>
            /// Compiles the custom conversions to a ParameterConverter instance.
            /// </summary>
            public ParameterConverter<TParams> Compile()
            {
                return new ParameterConverter<TParams>(_binderBuilders.Select(b => b.Compile(_withDefaults)));
            }

            /// <summary>
            /// Defines a conversion from a member value to a SQLite integer.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, long> toInteger)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion from a member value to a SQLite integer.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                Func<TField, long> toInteger)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            /// <summary>
            /// Defines a conversion from a member value to a SQLite float.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, double> toFloat)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion from a member value to a SQLite float.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                Func<TField, double> toFloat)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            /// <summary>
            /// Defines a conversion from a member value to a UTF-16 string.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="toText">Serializes a value to a destination Span&lt;char&gt;</param>
            /// <param name="length">Length of the Span&lt;char&gt; that a value will be serialized to.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToSpan<TField, char> toText,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Utf16Text(toText, length));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion from a member value to a UTF-16 string.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="canConvert"></param>
            /// <param name="toText">Serializes a value to a destination Span&lt;char&gt;</param>
            /// <param name="length">Length of the Span&lt;char&gt; that a value will be serialized to.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToSpan<TField, char> toText,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Utf16Text(canConvert, toText, length));
                return this;
            }

            /// <summary>
            /// Defines a reinterpret cast from a member value to a UTF-16 string.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="asText">A ReadOnlySpan&lt;char&gt; view of a value.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                AsSpan<TField, char> asText)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Utf16Text(asText));
                return this;
            }

            /// <summary>
            /// Defines a conditional reinterpret cast from a member value to a UTF-16 string.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="canConvert"></param>
            /// <param name="asText">A ReadOnlySpan&lt;char&gt; view of a value.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                AsSpan<TField, char> asText)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Utf16Text(canConvert, asText));
                return this;
            }

            /// <summary>
            /// Defines a conversion from a member value to a byte sequence.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="toBytes">Serializes a value to a destination Span&lt;byte&gt;</param>
            /// <param name="length">Length of the Span&lt;byte&gt; that a value will be serialized to.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToSpan<TField, byte> toBytes,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Blob(toBytes, length));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion from a member value to a byte sequence.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="canConvert"></param>
            /// <param name="toBytes">Serializes a value to a destination Span&lt;byte&gt;</param>
            /// <param name="length">Length of the Span&lt;byte&gt; that a value will be serialized to.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToSpan<TField, byte> toBytes,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Blob(canConvert, toBytes, length));
                return this;
            }

            /// <summary>
            /// Defines a reinterpret cast from a member value to a byte sequence.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="asBytes">A ReadOnlySpan&lt;byte&gt; view of a value.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                AsSpan<TField, byte> asBytes)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Blob(asBytes));
                return this;
            }

            /// <summary>
            /// Defines a conditional reinterpret cast from a member value to a byte sequence.
            /// </summary>
            /// <param name="propertyOrField"></param>
            /// <param name="canConvert"></param>
            /// <param name="asBytes">A ReadOnlySpan&lt;byte&gt; view of a value.</param>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                AsSpan<TField, byte> asBytes)
            {
                GetOrAdd(propertyOrField)
                    .Converters
                    .Add(ValueBinder.Converter.Blob(canConvert, asBytes));
                return this;
            }

            /// <summary>
            /// Defines a conversion from a member value to SQLite null.
            /// </summary>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null<TField>());
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion from a member value to SQLite null.
            /// </summary>
            /// <typeparam name="TField"></typeparam>
            /// <param name="propertyOrField"></param>
            /// <param name="canConvert"></param>
            /// <returns></returns>
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null(canConvert));
                return this;
            }

            /// <summary>
            /// Removes all defined conversions for a member.
            /// </summary>
            public Builder Ignore<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                MemberInfo member = GetGettableMember(propertyOrField);
                _binderBuilders.RemoveAll(builder => builder.Member == member);
                return this;
            }

            private static MemberInfo GetGettableMember<TField>(
                Expression<Func<TParams, TField>> propertyOrField)
            {
                if (propertyOrField.Body is MemberExpression memberExpression &&
                    CanGetValue(memberExpression.Member))
                {
                    return memberExpression.Member;
                }
                throw new ArgumentException($"Expression is not gettable field or property of {typeof(TParams).Name}");
            }

            private static bool CanGetValue(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanRead;
                }
                else if (member is FieldInfo field)
                {
                    return true;
                }
                return false;
            }
        }
    }
#nullable restore
}
