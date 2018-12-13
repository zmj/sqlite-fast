using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public readonly struct Row<TResult>
    {
        private readonly Row _row;
        private readonly ResultConverter<TResult> _converter;

        internal Row(Row row, ResultConverter<TResult> converter)
        {
            _row = row;
            _converter = converter;
        }

        public void AssignTo(ref TResult result)
        {
            foreach (Column col in _row.Columns)
            {
                IValueAssigner<TResult> assigner = _converter.ValueAssigners[col.Index]; // length checked in Statement
                assigner.Assign(ref result, col);
            }
        }
    }

    public readonly struct Rows<TResult> : IEnumerable<Row<TResult>>
    {
        private readonly Rows _rows;
        private readonly ResultConverter<TResult> _converter;

        internal Rows(Rows rows, ResultConverter<TResult> converter)
        {
            _rows = rows;
            _converter = converter;
        }

        public Enumerator GetEnumerator() => new Enumerator(_rows.GetEnumerator(), _converter);
        IEnumerator<Row<TResult>> IEnumerable<Row<TResult>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Row<TResult>>
        {
            private readonly ResultConverter<TResult> _converter;

            private Rows.Enumerator _rowEnumerator;

            internal Enumerator(Rows.Enumerator rowEnumerator, ResultConverter<TResult> converter)
            {
                _rowEnumerator = rowEnumerator;
                _converter = converter;
                Current = default;
            }

            public Row<TResult> Current { get; private set; }
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (!_rowEnumerator.MoveNext())
                {
                    Current = default;
                    return false;
                }
                Current = new Row<TResult>(_rowEnumerator.Current, _converter);
                return true;
            }

            public void Reset()
            {
                Current = default;
                _rowEnumerator.Reset();
            }

            public void Dispose()
            {
                _rowEnumerator = default;
            }
        }
    }
}
