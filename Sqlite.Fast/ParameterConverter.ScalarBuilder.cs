using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed partial class ParameterConverter<TParams>
    {
        public sealed class ScalarBuilder
        {
            private readonly ValueBinder.Builder<TParams, TParams> _builder;
            private readonly bool _withDefaults;

            public ScalarBuilder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _builder = ValueBinder.Build<TParams>();
            }

            public ParameterConverter<TParams> Compile() =>
                new ParameterConverter<TParams>(_builder.Compile(_withDefaults));

            public ScalarBuilder With(ToInteger<TParams> toInteger)
            {
                _builder.Converters.Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToInteger<TParams> toInteger)
            {
                _builder.Converters.Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            public ScalarBuilder With(ToFloat<TParams> toFloat)
            {
                _builder.Converters.Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToFloat<TParams> toFloat)
            {
                _builder.Converters.Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            public ScalarBuilder With(ToText<TParams> toText, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(toText, length));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToText<TParams> toText, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, toText, length));
                return this;
            }

            public ScalarBuilder With(AsText<TParams> asText)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(asText));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, AsText<TParams> asText)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, asText));
                return this;
            }

            public ScalarBuilder With(ToBlob<TParams> toBlob, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(toBlob, length));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToBlob<TParams> toBlob, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, toBlob, length));
                return this;
            }

            public ScalarBuilder With(AsBlob<TParams> asBlob)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(asBlob));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, AsBlob<TParams> asBlob)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, asBlob));
                return this;
            }
        }
    }
}
