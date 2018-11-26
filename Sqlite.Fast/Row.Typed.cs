using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public readonly struct Row<TRecord>
    {
        private readonly Row _row;
        private readonly Converter<TRecord> _converter;

        internal Row(Row row, Converter<TRecord> converter)
        {
            _row = row;
            _converter = converter;
        }

        public void AssignTo(ref TRecord record)
        {
            foreach (Column col in _row.Columns)
            {
                IValueAssigner<TRecord> assigner = _converter.ValueAssigners[col.Index]; // length checked in Statement
                assigner.AssignValue(ref record, col);
            }
        }
    }

    public readonly struct Rows<TRecord> : IEnumerable<Row<TRecord>>
    {
        private readonly Rows _rows;
        private readonly Converter<TRecord> _converter;

        internal Rows(Rows rows, Converter<TRecord> converter)
        {
            _rows = rows;
            _converter = converter;
        }

        public Enumerator GetEnumerator() => new Enumerator(_rows.GetEnumerator(), _converter);
        IEnumerator<Row<TRecord>> IEnumerable<Row<TRecord>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Row<TRecord>>
        {
            private readonly Converter<TRecord> _converter;

            private Rows.Enumerator _rowEnumerator;

            internal Enumerator(Rows.Enumerator rowEnumerator, Converter<TRecord> converter)
            {
                _rowEnumerator = rowEnumerator;
                _converter = converter;
                Current = default;
            }

            public Row<TRecord> Current { get; private set; }
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (!_rowEnumerator.MoveNext())
                {
                    Current = default;
                    return false;
                }
                Current = new Row<TRecord>(_rowEnumerator.Current, _converter);
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
