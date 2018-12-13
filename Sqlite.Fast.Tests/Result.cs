using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
#pragma warning disable CS0649
    internal struct R<T>
    {
        public T Value;

        private static readonly ResultConverter<R<T>> _converter
            = new ResultConverter<R<T>>.Builder().Compile();

        public ResultConverter<R<T>> C => _converter;
    }

    internal struct R<T, U>
    {
        public T Value1;
        public U Value2;

        private static readonly ResultConverter<R<T, U>> _converter
            = ResultConverter.Builder<R<T, U>>().Compile();

        public ResultConverter<R<T, U>> C => _converter;
    }
#pragma warning restore CS0649
}
