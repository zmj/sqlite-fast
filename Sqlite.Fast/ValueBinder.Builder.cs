using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    internal static class ValueBinder
    {
        internal interface IBuilder<TParams>
        {
            MemberInfo Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo Member { get; }

            public IValueBinder<TParams> Compile(bool withDefaults)
            {
                throw new NotImplementedException();
            }

            public Builder<TParams, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TParams, TCallerField> builder)
                {
                    return builder;
                }
                throw new InvalidOperationException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }
    }
}
