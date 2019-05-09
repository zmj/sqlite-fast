using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct Row
    {
        private readonly IntPtr _statement;

        public Row(IntPtr statement) => _statement = statement;

        public Column this[int index] => new Column(_statement, index);
    }

    internal readonly struct Rows
    {
        private readonly IntPtr _statement;

        public Rows(IntPtr statement) => _statement = statement;

        public Enumerator GetEnumerator() => new Enumerator(_statement);

        public readonly struct Enumerator
        {
            private readonly IntPtr _statement;

            public Enumerator(IntPtr statement)
            {
                _statement = statement;
                Current = new Row(statement);
            }

            public Row Current { get; }

            public bool MoveNext()
            {
                Sqlite.Result r = Sqlite.Step(_statement);
                if (r == Sqlite.Result.Row)
                {
                    return true;
                }
                r.ThrowIfNot(Sqlite.Result.Done, nameof(Sqlite.Step));
                return false;
            }

            public void Reset()
            {
                Sqlite.Reset(_statement).ThrowIfNotOK(nameof(Sqlite.Reset));
            }

            public void Dispose() => Reset();
        }
    }
}
