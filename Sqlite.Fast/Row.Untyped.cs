﻿using System;
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
        private readonly VersionToken _version;

        public Rows(IntPtr statement, int columnCount, VersionToken version)
        {
            _statement = statement;
            _columnCount = columnCount;
            _version = version;
        }

        public Enumerator GetEnumerator() => new Enumerator(_statement, _columnCount, _version);

        public struct Enumerator
        {
            private readonly IntPtr _statement;
            private readonly int _columnCount;
            private readonly VersionToken _version;

            public Enumerator(IntPtr statement, int columnCount, VersionToken version)
            {
                _statement = statement;
                _columnCount = columnCount;
                _version = version;
                Current = default;
            }

            public Row Current { get; private set; }

            public bool MoveNext()
            {
                CheckVersion();
                Sqlite.Result r = Sqlite.Step(_statement);
                if (r == Sqlite.Result.Row)
                {
                    Current = new Row(_statement, _columnCount);
                    return true;
                }
                else if (r == Sqlite.Result.Done)
                {
                    Current = default;
                    return false;
                }
                else
                {
                    throw new SqliteException(r, "Statement execution failed");
                }
            }

            public void Reset()
            {
                CheckVersion();
                Current = default;
                Sqlite.Reset(_statement);
            }

            private void CheckVersion()
            {
                if (_version.IsStale)
                {
                    throw new InvalidOperationException("Result enumerator used after source statement reset");
                }
            }
        }
    }
}