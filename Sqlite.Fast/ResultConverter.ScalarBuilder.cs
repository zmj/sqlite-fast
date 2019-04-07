using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
#nullable enable
    /// <summary>
    /// ResultConverter assigns a SQLite result row to an instance of the result type.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed partial class ResultConverter<TResult>
    {
        /// <summary>
        /// ResultConverter.ScalarBuilder constructs a custom ResultConverter.
        /// </summary>
        public sealed class ScalarBuilder
        {
            private readonly ValueAssigner.Builder<TResult, TResult> _builder;
            private readonly bool _withDefaults;

            /// <summary>
            /// Creates a builder for a custom ResultConverter.
            /// Call builder.With(...) to define conversions, then builder.Compile().
            /// </summary>
            /// <param name="withDefaultConversions">If true, conversion will fall back to default conversion when no custom conversion is defined.</param>
            public ScalarBuilder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _builder = ValueAssigner.Build<TResult>();
            }

            /// <summary>
            /// Compiles the custom conversions to a ResultConverter instance.
            /// </summary>
            public ResultConverter<TResult> Compile() =>
                new ResultConverter<TResult>(_builder.Compile(_withDefaults));
            
            /// <summary>
            /// Defines a conversion from a SQLite integer.
            /// </summary>
            public ScalarBuilder With(Func<long, TResult> fromInteger)
            {
                _builder.FromInteger = fromInteger;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a SQLite float.
            /// </summary>
            public ScalarBuilder With(Func<double, TResult> fromFloat)
            {
                _builder.FromFloat = fromFloat;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a UTF-16 string.
            /// </summary>
            /// <param name="fromText">Deserializes a value from a source ReadOnlySpan&lt;char&gt;</param>
            public ScalarBuilder With(FromSpan<char, TResult> fromText)
            {
                _builder.FromUtf16Text = fromText;
                return this;
            }

            /// <summary>
            /// Defines a conversion from a byte sequence.
            /// </summary>
            /// <param name="fromBytes">Deserializes a value from a source ReadOnlySpan&lt;byte&gt;</param>
            public ScalarBuilder With(FromSpan<byte, TResult> fromBytes)
            {
                _builder.FromBlob = fromBytes;
                return this;
            }

            /// <summary>
            /// Defines a conversion from SQLite null.
            /// </summary>
            public ScalarBuilder With(Func<TResult> fromNull)
            {
                _builder.FromNull = fromNull;
                return this;
            }
        }
    }
#nullable restore
}
