using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed partial class ResultConverter<TResult>
    {
        public sealed class ScalarBuilder
        {
            private readonly ValueAssigner.Builder<TResult, TResult> _builder;
            private readonly bool _withDefaults;

            public ScalarBuilder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _builder = ValueAssigner.Build<TResult>();
            }

            public ResultConverter<TResult> Compile() =>
                new ResultConverter<TResult>(_builder.Compile(_withDefaults));

            public ScalarBuilder With(FromInteger<TResult> fromInteger)
            {
                _builder.FromInteger = fromInteger;
                return this;
            }

            public ScalarBuilder With(FromFloat<TResult> fromFloat)
            {
                _builder.FromFloat = fromFloat;
                return this;
            }

            public ScalarBuilder With(FromText<TResult> fromText)
            {
                _builder.FromUtf16Text = fromText;
                return this;
            }

            public ScalarBuilder With(FromBlob<TResult> fromBlob)
            {
                _builder.FromBlob = fromBlob;
                return this;
            }

            public ScalarBuilder With(FromNull<TResult> fromNull)
            {
                _builder.FromNull = fromNull;
                return this;
            }
        }
    }
}
