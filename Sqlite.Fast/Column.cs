using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Column
    {
        private readonly IntPtr _statement;
        private readonly VersionToken _version;

        public Column(IntPtr statement, int index, VersionToken version)
        {
            _statement = statement;
            Index = index;
            _version = version;
        }

        public int Index { get; }

        public DataType GetDataType()
        {
            CheckVersion();
            return Sqlite.ColumnType(_statement, Index);
        }

        public long GetIntegerData()
        {
            CheckVersion();
            return Sqlite.ColumnInteger(_statement, Index);
        }

        public double GetFloatData()
        {
            CheckVersion();
            return Sqlite.ColumnFloat(_statement, Index);
        }

        public Span<char> GetTextData()
        {
            CheckVersion();
            IntPtr data = Sqlite.ColumnText16(_statement, Index);
            int length = Sqlite.ColumnBytes16(_statement, Index) >> 1;
            unsafe
            {
                return new Span<char>(data.ToPointer(), length);
            }
        }

        public Span<byte> GetBlobData()
        {
            CheckVersion();
            IntPtr data = Sqlite.ColumnBlob(_statement, Index);
            int length = Sqlite.ColumnBytes(_statement, Index);
            unsafe
            {
                return new Span<byte>(data.ToPointer(), length);
            }
        }

        private void CheckVersion()
        {
            if (_version.IsStale)
            {
                throw new InvalidOperationException("Result used after source statement reset");
            }
        }
    }

    internal readonly struct Columns
    {
        private readonly IntPtr _statement;
        private readonly int _columnCount;
        private readonly VersionToken _version;

        public Columns(IntPtr statement, int columnCount, VersionToken version)
        {
            _statement = statement;
            _columnCount = columnCount;
            _version = version;
        }

        public Enumerator GetEnumerator() => new Enumerator(_statement, _columnCount, _version);

        internal struct Enumerator
        {
            private readonly IntPtr _statement;
            private readonly int _columnCount;
            private readonly VersionToken _version;

            private int _columnIndex;

            public Enumerator(IntPtr statement, int columnCount, VersionToken version)
            {
                _statement = statement;
                _columnCount = columnCount;
                _version = version;
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
                Current = new Column(_statement, _columnIndex, _version);
                return true;
            }
        }
    }
}
