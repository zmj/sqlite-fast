using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class ParameterConverter<TParams>
    {
        internal readonly IValueBinder<TParams>[] ValueBinders;

        internal ParameterConverter(IEnumerable<IValueBinder<TParams>> valueBinders)
        {
            ValueBinders = valueBinders.ToArray();
        }

        public sealed class Builder
        {
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
            }

            public ParameterConverter<TParams> Compile()
            {
                throw new NotImplementedException();
            }
        }
    }

    public static class ParameterConverter
    {
        public static ParameterConverter<TParams>.Builder Builder<TParams>(bool withDefaultConversions = true)
            => new ParameterConverter<TParams>.Builder(withDefaultConversions);
    }
}
