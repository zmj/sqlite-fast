using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
    internal struct R<T>
    {
        public T Value;

        private static readonly Converter<R<T>> Converter
            = new Converter<R<T>>.Builder().Compile();

        public Converter<R<T>> C => Converter;
    }
}
