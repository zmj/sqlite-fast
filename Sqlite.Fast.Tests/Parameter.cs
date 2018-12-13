using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
#pragma warning disable CS0649
    internal struct P<T>
    {
        public T Value;

        private static readonly ParameterConverter<P<T>> _converter
            = new ParameterConverter<P<T>>.Builder().Compile();

        public ParameterConverter<P<T>> C => _converter;
    }

    internal struct P<T, U>
    {
        public T Value1;
        public U Value2;

        private static readonly ParameterConverter<P<T, U>> _converter
            = ParameterConverter.Builder<P<T, U>>().Compile();

        public ParameterConverter<P<T, U>> C => _converter;
    }
#pragma warning restore CS0649
}
