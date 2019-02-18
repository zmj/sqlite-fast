using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Row
    {
        public readonly Columns Columns;

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
                Sqlite.Result r = Sqlite.Step(_statement);
                if (r == Sqlite.Result.Row)
                {
                    Current = new Row(_statement, _columnCount);
                    return true;
                }
                r.ThrowIfNot(Sqlite.Result.Done, nameof(Sqlite.Step));
                Reset();
                return false;
            }

            public void Reset()
            {
                Sqlite.Reset(_statement).ThrowIfNotOK(nameof(Sqlite.Reset));
                Current = default;
            }

            public void Dispose() => Reset();
        }
    }
}
