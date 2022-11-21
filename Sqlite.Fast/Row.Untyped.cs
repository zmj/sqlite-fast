using SQLitePCL;
using SQLitePCL.Ugly;
using System;

namespace Sqlite.Fast
{
    internal readonly struct Row
    {
        private readonly sqlite3_stmt _statement;

        public Row(sqlite3_stmt statement) => _statement = statement;

        public Column this[int index] => new Column( _statement, index);
    }

    internal readonly struct Rows
    {
        private readonly sqlite3_stmt _statement;

        public Rows(sqlite3_stmt statement) => _statement = statement;

        public Enumerator GetEnumerator() => new Enumerator(_statement);

        public readonly struct Enumerator
        {
            private readonly sqlite3_stmt _statement;

            public Enumerator(sqlite3_stmt statement)
            {
                _statement = statement;
                Current = new Row(statement);
            }

            public Row Current { get; }

            public bool MoveNext()
            {
                var r = (Sqlite.Result)_statement.step();
                if (r == Sqlite.Result.Row)
                {
                    return true;
                }

                if (r == Sqlite.Result.Done)
                {
                    return false;
                }

                throw new ugly.sqlite3_exception((int)r);
            }

            public void Reset() => _statement.reset();

            public void Dispose() => Reset();
        }
    }
}
