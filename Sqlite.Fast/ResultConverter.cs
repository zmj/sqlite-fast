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

    public static class ResultConverter
    {
        public static ResultConverter<TResult>.Builder Builder<TResult>(bool withDefaultConversions = true) =>
            new ResultConverter<TResult>.Builder(withDefaultConversions);

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
