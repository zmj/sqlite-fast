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
        private readonly Rows.Enumerator _rowEnumerator;
        private readonly ResultConverter<TResult> _converter;

        internal Row(Rows.Enumerator rowEnumerator, ResultConverter<TResult> converter)
        {
            _rowEnumerator = rowEnumerator;
            _converter = converter;
        }

        /// <summary>
        /// Assigns this row's values to an instance of the result type.
        /// </summary>
        public void AssignTo(out TResult result) => 
            _converter.AssignTo(out result, _rowEnumerator.Current);
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
        public readonly struct Enumerator : IEnumerator<Row<TResult>>
        {
            private readonly Rows.Enumerator _rowEnumerator;

            internal Enumerator(
                Rows.Enumerator rowEnumerator,
                ResultConverter<TResult> converter)
            {
                _rowEnumerator = rowEnumerator;
                Current = new Row<TResult>(rowEnumerator, converter);
            }

            /// <summary>
            /// The current result row. Undefined if MoveNext has not been called or returned false on the most recent call.
            /// </summary>
            public Row<TResult> Current { get; }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Attempts to step to the next result row.
            /// </summary>
            /// <returns>False if there are no more result rows; otherwise true.</returns>
            public bool MoveNext() => _rowEnumerator.MoveNext();

            /// <summary>
            /// Resets the result enumerator and underlying statement.
            /// </summary>
            public void Reset() => _rowEnumerator.Reset();

            /// <summary>
            /// Resets the underlying statement.
            /// </summary>
            public void Dispose() => _rowEnumerator.Dispose();
        }
    }
}
