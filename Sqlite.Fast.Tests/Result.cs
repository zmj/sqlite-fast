using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
#pragma warning disable CS0649
    internal struct R<T>
    {
        public T Value;

        private static readonly RecordConverter<R<T>> _converter
            = new RecordConverter<R<T>>.Builder().Compile();

        public RecordConverter<R<T>> C => _converter;
    }

    internal struct R<T, U>
    {
        public T Value1;
        public U Value2;

        private static readonly RecordConverter<R<T, U>> _converter
            = RecordConverter.Builder<R<T, U>>().Compile();

        public RecordConverter<R<T, U>> C => _converter;
    }
#pragma warning restore CS0649
}
