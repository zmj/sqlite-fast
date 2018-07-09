using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Column
    {
        private readonly IntPtr _statement;

        public Column(IntPtr statement, int index)
        {
            _statement = statement;
            Index = index;
        }

        public int Index { get; }

        public DataType GetDataType() => Sqlite.ColumnType(_statement, Index);

        public long GetIntegerData() => Sqlite.ColumnInteger(_statement, Index);

        public double GetFloatData() => Sqlite.ColumnFloat(_statement, Index);

        public Span<byte> GetTextData()
        {
            IntPtr data = Sqlite.ColumnText(_statement, Index);
            int length = Sqlite.ColumnBytes(_statement, Index);
            unsafe
            {
                return new Span<byte>(data.ToPointer(), length);
            }
        }

        public Span<byte> GetBlobData()
        {
            IntPtr data = Sqlite.ColumnBlob(_statement, Index);
            int length = Sqlite.ColumnBytes(_statement, Index);
            unsafe
            {
                return new Span<byte>(data.ToPointer(), length);
            }
        }
    }

    internal readonly struct Columns
    {
        private readonly IntPtr _statement;
        private readonly int _columnCount;

        public Columns(IntPtr statement, int columnCount)
        {
            _statement = statement;
            _columnCount = columnCount;
        }

        public Enumerator GetEnumerator() => new Enumerator(_statement, _columnCount);

        internal struct Enumerator
        {
            private readonly IntPtr _statement;
            private readonly int _columnCount;

            private int _columnIndex;

            public Enumerator(IntPtr statement, int columnCount)
            {
                _statement = statement;
                _columnCount = columnCount;
                _columnIndex = -1;
                Current = default;
            }

            public Column Current { get; private set; }

            public bool MoveNext()
            {
                _columnIndex++;
                if(_columnIndex >= _columnCount)
                {
                    Current = default;
                    return false;
                }
                Current = new Column(_statement, _columnIndex);
                return true;
            }
        }
    }
}
