using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Column
    {
        public readonly int Index;
        public readonly Sqlite.DataType DataType;
        private readonly IntPtr _statement;

        public Column(IntPtr statement, int index)
        {
            _statement = statement;
            Index = index;
            DataType = Sqlite.ColumnType(statement, index);
        }

        public long AsInteger() => Sqlite.ColumnInt64(_statement, Index);

        public double AsFloat() => Sqlite.ColumnDouble(_statement, Index);

        public ReadOnlySpan<byte> AsUtf8Text()
        {
            IntPtr data = Sqlite.ColumnText(_statement, Index);
            int length = Sqlite.ColumnBytes(_statement, Index);
            unsafe
            {
                return new ReadOnlySpan<byte>(data.ToPointer(), length);
            }
        }

        public ReadOnlySpan<char> AsUtf16Text()
        {
            IntPtr data = Sqlite.ColumnText16(_statement, Index);
            int length = Sqlite.ColumnBytes16(_statement, Index) >> 1;
            unsafe
            {
                return new Span<char>(data.ToPointer(), length);
            }
        }

        public ReadOnlySpan<byte> AsBlob()
        {
            IntPtr data = Sqlite.ColumnBlob(_statement, Index);
            int length = Sqlite.ColumnBytes(_statement, Index);
            unsafe
            {
                return new Span<byte>(data.ToPointer(), length);
            }
        }
    }
}
