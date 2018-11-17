using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
    internal struct R<T>
    {
        public T Value;

        private static readonly RowToRecordMap<R<T>> TypeMap
            = RowToRecordMap.Default<R<T>>().Compile();

        public RowToRecordMap<R<T>> Map => TypeMap;
    }

}
