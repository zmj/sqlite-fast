using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Row is a single result of a statement.
    /// </summary>
    /// <typeparam name="TResult">The type that this result row can be assigned to.</typeparam>
    public readonly struct Row<TResult>
    {
        private readonly Row _row;
        private readonly ResultConverter<TResult> _converter;

        internal Row(Row row, ResultConverter<TResult> converter)
        {
            _row = row;
            _converter = converter;
        }

        /// <summary>
        /// Assigns this row's values to an instance of the result type.
        /// </summary>
        public void AssignTo(out TResult result) => _converter.AssignTo(out result, _row.Columns);
    }

    /// <summary>
    /// Rows is an enumeration of the results of a statement.
    /// </summary>
    /// <typeparam name="TResult">The type that the result rows can be assigned to.</typeparam>
    public readonly struct Rows<TResult> : IEnumerable<Row<TResult>>
    {
        private readonly Rows _rows;
        private readonly ResultConverter<TResult> _converter;

        internal Rows(Rows rows, ResultConverter<TResult> converter)
        {
            _rows = rows;
            _converter = converter;
        }

        /// <summary>
        /// Gets a result row enumerator.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(_rows.GetEnumerator(), _converter);

        IEnumerator<Row<TResult>> IEnumerable<Row<TResult>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerator steps through the result rows of a statement.
        /// The statement is reset when enumeration finishes or the enumerator is disposed.
        /// </summary>
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

            /// <summary>
            /// The current result row. Default if MoveNext has not been called or returned false on the most recent call.
            /// </summary>
            public Row<TResult> Current { get; private set; }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Attempts to step to the next result row.
            /// </summary>
            /// <returns>False if there are no more result rows; otherwise true.</returns>
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

            /// <summary>
            /// Resets the result enumerator and underlying statement.
            /// </summary>
            public void Reset()
            {
                Current = default;
                _rowEnumerator.Reset();
            }

            /// <summary>
            /// Resets the underlying statement.
            /// </summary>
            public void Dispose()
            {
                _rowEnumerator.Dispose();
                _rowEnumerator = default;
            }
        }
    }
}
