using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal interface IValueBinder<TParams>
    {
        void Bind(ref TParams parameters, Statement statement, int index);
    }

    internal sealed class ValueBinder<TParams, TField> : IValueBinder<TParams>
    {
        public void Bind(ref TParams parameters, Statement statement, int index)
        {
            throw new NotImplementedException();
        }
    }
}
