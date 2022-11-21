using SQLitePCL;
using SQLitePCL.Ugly;
using System;
using System.Runtime.InteropServices;

namespace Sqlite.Fast
{
    internal readonly struct Column
    {
        public readonly int Index;
        public readonly Sqlite.DataType DataType;
        private readonly sqlite3_stmt _statement;

        public Column(sqlite3_stmt statement, int index)
        {
            _statement = statement;
            Index = index;
            DataType = (Sqlite.DataType)statement.column_type(index);
        }

        public long AsInteger() => _statement.column_int64(Index);

        public double AsFloat() => _statement.column_double(Index);

        public ReadOnlySpan<byte> AsUtf8Text()
        {
            var wrapper = raw.sqlite3_column_text(_statement, Index);
            var nullTerminatedBytes = new U { Wrapper = wrapper }.Bytes;
            if (nullTerminatedBytes.IsEmpty)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            return nullTerminatedBytes.Slice(0, nullTerminatedBytes.Length - 1);
        }

        [StructLayout(LayoutKind.Explicit)]
        private ref struct U
        {
            [FieldOffset(0)]
            public utf8z Wrapper;
            
            [FieldOffset(0)]
            public ReadOnlySpan<byte> Bytes;
        }

        public ReadOnlyMemory<char> AsUtf16Text()
        {
            // workaround: text16 not exposed in raw
            return _statement.column_text(Index).AsMemory();
        }

        public ReadOnlySpan<byte> AsBlob() => _statement.column_blob(Index);
    }
}
