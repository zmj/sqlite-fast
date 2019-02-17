using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqlite.Fast
{
    public sealed partial class ResultConverter<TResult>
    {
        private readonly IValueAssigner<TResult>[] _assigners;
        private readonly ValueAssigner<TResult, TResult> _scalarAssigner;
        internal readonly int FieldCount;

        internal ResultConverter(IEnumerable<IValueAssigner<TResult>> valueAssigners)
        {
            _assigners = valueAssigners.ToArray();
            FieldCount = _assigners.Length;
        }

        internal ResultConverter(ValueAssigner<TResult, TResult> scalarAssigner)
        {
            _scalarAssigner = scalarAssigner;
            FieldCount = 1;
        }

        internal void AssignValues(ref TResult result, Columns columns)
        {
            // counts verified equal before execution
            if (_scalarAssigner != null)
            {
                var enumerator = columns.GetEnumerator();
                enumerator.MoveNext();
                _scalarAssigner.Assign(ref result, enumerator.Current);
            }
            else
            { 
                foreach (Column col in columns)
                {
                    _assigners[col.Index].Assign(ref result, col);
                }
            }
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
        public static ResultConverter<TResult>.Builder Builder<TResult>(bool withDefaultConversions = true) =>
            new ResultConverter<TResult>.Builder(withDefaultConversions);

        /// <summary>
        /// Creates a builder for a custom ResultConverter.
        /// Call builder.With(...) to define conversions, then builder.Compile().
        /// </summary>
        /// <param name="withDefaultConversions">If true, conversion will fall back to default conversion when no custom conversion is defined.</param>
        public static ResultConverter<TResult>.ScalarBuilder ScalarBuilder<TResult>(bool withDefaultConversions = true) =>
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
