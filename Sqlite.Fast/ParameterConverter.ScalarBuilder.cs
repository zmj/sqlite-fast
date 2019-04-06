using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed partial class ParameterConverter<TParams>
    {
        /// <summary>
        /// ParameterConverter.ScalarBuilder constructs a custom ParameterConverter.
        /// </summary>
        public sealed class ScalarBuilder
        {
            private readonly ValueBinder.Builder<TParams, TParams> _builder;
            private readonly bool _withDefaults;

            /// <summary>
            /// Creates a builder for a custom ParameterConverter.
            /// Call builder.With(...) to define conversions, then builder.Compile().
            /// </summary>
            /// <param name="withDefaultConversions">If true, conversion will fall back to default conversions when no custom conversion can be used.</param>
            public ScalarBuilder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _builder = ValueBinder.Build<TParams>();
            }

            /// <summary>
            /// Compiles the custom conversions to a ParameterConverter instance.
            /// </summary>
            public ParameterConverter<TParams> Compile() =>
                new ParameterConverter<TParams>(_builder.Compile(_withDefaults));

            /// <summary>
            /// Defines a conversion to a SQLite integer.
            /// </summary>
            public ScalarBuilder With(Func<TParams, long> toInteger)
            {
                toInteger.ThrowIfNull(nameof(toInteger));
                _builder.Converters.Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion to a SQLite integer.
            /// </summary>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert, 
                Func<TParams, long> toInteger)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                toInteger.ThrowIfNull(nameof(toInteger));
                _builder.Converters.Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            /// <summary>
            /// Defines a conversion to a SQLite float.
            /// </summary>
            public ScalarBuilder With(Func<TParams, double> toFloat)
            {
                toFloat.ThrowIfNull(nameof(toFloat));
                _builder.Converters.Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion to a SQLite float.
            /// </summary>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert,
                Func<TParams, double> toFloat)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                toFloat.ThrowIfNull(nameof(toFloat));
                _builder.Converters.Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            /// <summary>
            /// Defines a conversion to a UTF-16 string.
            /// </summary>
            /// <param name="toText">Serializes a value to a destination Span&lt;char&gt;</param>
            /// <param name="length">Length of the Span&lt;char&gt; that a value will be serialized to.</param>
            public ScalarBuilder With(
                ToSpan<TParams, char> toText,
                Func<TParams, int> length)
            {
                toText.ThrowIfNull(nameof(toText));
                length.ThrowIfNull(nameof(length));
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(toText, length));
                return this;
            }

            /// <summary>
            /// Defines a conditional conversion to a UTF-16 string.
            /// </summary>
            /// <param name="canConvert"></param>
            /// <param name="toText">Serializes a value to a destination Span&lt;char&gt;</param>
            /// <param name="length">Length of the Span&lt;char&gt; that a value will be serialized to.</param>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert,
                ToSpan<TParams, char> toText,
                Func<TParams, int> length)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                toText.ThrowIfNull(nameof(toText));
                length.ThrowIfNull(nameof(length));
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, toText, length));
                return this;
            }

            /// <summary>
            /// Defines a reinterpret cast to a UTF-16 string.
            /// </summary>
            /// <param name="asText">A ReadOnlySpan&lt;char&gt; view of a value.</param>
            public ScalarBuilder With(AsSpan<TParams, char> asText)
            {
                asText.ThrowIfNull(nameof(asText));
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(asText));
                return this;
            }

            /// <summary>
            /// Defines a conditional reinterpret cast to a UTF-16 string.
            /// </summary>
            /// <param name="canConvert"></param>
            /// <param name="asText">A ReadOnlySpan&lt;char&gt; view of a value.</param>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert, 
                AsSpan<TParams, char> asText)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                asText.ThrowIfNull(nameof(asText));
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, asText));
                return this;
            }

            /// <summary>
            /// Defines a conversion to a byte sequence.
            /// </summary>
            /// <param name="toBytes">Serializes a value to a destination Span&lt;byte&gt;</param>
            /// <param name="length">Length of the Span&lt;byte&gt; that a value will be serialized to.</param>
            public ScalarBuilder With(
                ToSpan<TParams, byte> toBytes, 
                Func<TParams, int> length)
            {
                toBytes.ThrowIfNull(nameof(toBytes));
                length.ThrowIfNull(nameof(length));
                _builder.Converters.Add(ValueBinder.Converter.Blob(toBytes, length));
                return this;
            }


            /// <summary>
            /// Defines a conditional conversion to a byte sequence.
            /// </summary>
            /// <param name="canConvert"></param>
            /// <param name="toBytes">Serializes a value to a destination Span&lt;byte&gt;</param>
            /// <param name="length">Length of the Span&lt;byte&gt; that a value will be serialized to.</param>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert, 
                ToSpan<TParams, byte> toBytes, 
                Func<TParams, int> length)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                toBytes.ThrowIfNull(nameof(toBytes));
                length.ThrowIfNull(nameof(length));
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, toBytes, length));
                return this;
            }

            /// <summary>
            /// Defines a reinterpret cast to a byte sequence.
            /// </summary>
            /// <param name="asBytes">A ReadOnlySpan&lt;byte&gt; view of a value.</param>
            public ScalarBuilder With(AsSpan<TParams, byte> asBytes)
            {
                asBytes.ThrowIfNull(nameof(asBytes));
                _builder.Converters.Add(ValueBinder.Converter.Blob(asBytes));
                return this;
            }

            /// <summary>
            /// Defines a conditional reinterpret cast to a byte sequence.
            /// </summary>
            /// <param name="canConvert"></param>
            /// <param name="asBytes">A ReadOnlySpan&lt;byte&gt; view of a value.</param>
            public ScalarBuilder With(
                Func<TParams, bool> canConvert,
                AsSpan<TParams, byte> asBytes)
            {
                canConvert.ThrowIfNull(nameof(canConvert));
                asBytes.ThrowIfNull(nameof(asBytes));
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, asBytes));
                return this;
            }
        }
    }
}
