using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Row
    {
        public Columns Columns { get; }

        public Row(IntPtr statement, int columnCount)
        {
            Columns = new Columns(statement, columnCount);
        }
    }

    internal readonly struct Rows
    {
        private readonly IntPtr _statement;
        private readonly int _columnCount;

        public Rows(IntPtr statement, int columnCount)
        {
            _statement = statement;
            _columnCount = columnCount;
        }

        public Enumerator GetEnumerator() => new Enumerator(_statement, _columnCount);

        public struct Enumerator
        {
            private readonly IntPtr _statement;
            private readonly int _columnCount;

            public Enumerator(IntPtr statement, int columnCount)
            {
                _statement = statement;
                _columnCount = columnCount;
                Current = default;
            }

            public Row Current { get; private set; }

            public bool MoveNext()
            {
                Result r = Sqlite.Step(_statement);
                if (r != Result.Row)
                {
                    r = Sqlite.Reset(_statement);
                    if (r.IsError())
                    {
                        throw new SqliteException(r, "Query execution failed");
                    }
                    return false;
                }
                Current = new Row(_statement, _columnCount);
                return true;
            }

            public void Reset()
            {
                Current = default;
                Sqlite.Reset(_statement);
            }
        }
    }
}
