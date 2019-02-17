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
        private readonly StatementResetter _resetter;

        public Rows(IntPtr statement, int columnCount, StatementResetter resetter)
        {
            _statement = statement;
            _columnCount = columnCount;
            _resetter = resetter;
        }

        public Enumerator GetEnumerator() => new Enumerator(_statement, _columnCount, _resetter);

        public struct Enumerator
        {
            private readonly IntPtr _statement;
            private readonly int _columnCount;
            private readonly StatementResetter _resetter;

            private StatementVersion? _enumerationVersion;

            public Enumerator(IntPtr statement, int columnCount, StatementResetter resetter)
            {
                _statement = statement;
                _columnCount = columnCount;
                _resetter = resetter;
                _enumerationVersion = null;
                Current = default;
            }

            public Row Current { get; private set; }

            public bool MoveNext()
            {
                if (_enumerationVersion.HasValue) _resetter.ThrowIfReset(expectedVersion: _enumerationVersion.Value);
                else _enumerationVersion = _resetter.CurrentVersion;

                Sqlite.Result r = Sqlite.Step(_statement);
                if (r == Sqlite.Result.Done)
                {
                    Reset();
                    return false;
                }
                r.ThrowIfNot(Sqlite.Result.Row, nameof(Sqlite.Step));
                Current = new Row(_statement, _columnCount);
                return true;
            }

            public void Reset()
            {
                if (_enumerationVersion.HasValue)
                {
                    _resetter.ResetIfNecessary(lastKnownVersion: _enumerationVersion.Value);
                    _enumerationVersion = null;
                }
                Current = default;
            }

            public void Dispose() => Reset();
        }
    }
}
