using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public sealed partial class ParameterConverter<TParams>
    {
        private readonly IValueBinder<TParams>[] _binders;
        private readonly ValueBinder<TParams, TParams> _scalarBinder;
        internal readonly int FieldCount;

        internal ParameterConverter(IEnumerable<IValueBinder<TParams>> valueBinders)
        {
            _binders = valueBinders.ToArray();
            FieldCount = _binders.Length;
        }

        internal ParameterConverter(ValueBinder<TParams, TParams> scalarBinder)
        {
            _scalarBinder = scalarBinder;
            FieldCount = 1;
        }

        internal void BindValues(in TParams parameters, Statement statement)
        {
            // parameters are 1-indexed
            if (_scalarBinder != null)
            {
                _scalarBinder.Bind(in parameters, statement, index: 1);
            }
            else
            {
                for (int i = 0; i < _binders.Length; i++)
                {
                    _binders[i].Bind(in parameters, statement, i + 1);
                }
            }
        }
    }

    public static class ParameterConverter
    {
        public static ParameterConverter<TParams>.Builder Builder<TParams>(bool withDefaultConversions = true) =>
            new ParameterConverter<TParams>.Builder(withDefaultConversions);

        public static ParameterConverter<TParams>.ScalarBuilder ScalarBuilder<TParams>(bool withDefaultConversions = true) =>
            new ParameterConverter<TParams>.ScalarBuilder(withDefaultConversions);

        internal static ParameterConverter<TParams> Default<TParams>()
        {
            if (typeof(TParams).IsScalar())
            {
                return ScalarBuilder<TParams>().Compile();
            }
            else
            {
                return Builder<TParams>().Compile();
            }
        }
    }
}
