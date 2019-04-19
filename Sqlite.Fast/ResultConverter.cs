using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqlite.Fast
{
    public sealed partial class ResultConverter<TResult>
    {
        // null if scalar or non-scalar value type
        private readonly Func<TResult>? _initializer;

        // null if scalar
        private readonly IValueAssigner<TResult>[]? _assigners;

        // null if non-scalar
        private readonly ValueAssigner<TResult, TResult>? _scalarAssigner;

        internal readonly int FieldCount;

        internal ResultConverter(
            Func<TResult>? initalizer,
            IEnumerable<IValueAssigner<TResult>> valueAssigners)
        {
            _initializer = initalizer;
            _assigners = valueAssigners.ToArray();
            FieldCount = _assigners.Length;
        }

        internal ResultConverter(ValueAssigner<TResult, TResult> scalarAssigner)
        {
            _scalarAssigner = scalarAssigner;
            FieldCount = 1;
        }

        internal void AssignTo(out TResult result, in Columns columns)
        {
            // todo:
            // * index accessor instead of enumerator
            // * assigner implementation?
            if (_scalarAssigner != null)
            {
                AssignScalar(out result, columns);
            }
            else
            {
                AssignValues(out result, columns);
            }
        }

        private void AssignValues(out TResult result, in Columns columns)
        {
            result = _initializer != null ? _initializer() : default!;
            // counts verified equal before assignment
            foreach (Column col in columns)
            {
                _assigners![col.Index].Assign(ref result, col);
            }
        }

        private void AssignScalar(out TResult result, in Columns columns)
        {
            var enumerator = columns.GetEnumerator();
            enumerator.MoveNext();
            TResult value = default!;
            _scalarAssigner!.Assign(ref value!, enumerator.Current);
            result = value;
        }
    }

    /// <summary>
    /// ResultConverter assigns a SQLite result row to an instance of the result type.
    /// </summary>
    public static class ResultConverter
    {
        /// <summary>
        /// Creates a builder for a custom ResultConverter.
        /// Call builder.With(...) to define member conversions, then builder.Compile().
        /// </summary>
        /// <param name="withDefaultConversions">If true, member conversions will fall back to default conversion when no custom conversion is defined.</param>
        public static ResultConverter<TResult>.Builder Builder<TResult>(
            bool withDefaultConversions = true) =>
            new ResultConverter<TResult>.Builder(withDefaultConversions);

        /// <summary>
        /// Creates a builder for a custom ResultConverter.
        /// Call builder.With(...) to define conversions, then builder.Compile().
        /// </summary>
        /// <param name="withDefaultConversions">If true, conversion will fall back to default conversion when no custom conversion is defined.</param>
        public static ResultConverter<TResult>.ScalarBuilder ScalarBuilder<TResult>(
            bool withDefaultConversions = true) =>
            new ResultConverter<TResult>.ScalarBuilder(withDefaultConversions);

        internal static ResultConverter<TResult> Default<TResult>()
        {
            if (typeof(TResult).IsScalar())
            {
                return ScalarBuilder<TResult>().Compile();
            }
            else
            {
                return Builder<TResult>().Compile();
            }
        }
    }
}
