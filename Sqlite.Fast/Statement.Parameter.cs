using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TParams> : IDisposable
    {
        private readonly Statement _statement;
        private readonly ParameterConverter<TParams> _converter;

        internal Statement(Statement statement, ParameterConverter<TParams> converter)
        {
            _statement = statement;
            _converter = converter;
            ValidateConverter(converter);
        }

        public Statement<TParams> Bind(in TParams parameters)
            => BindInternal(_converter, in parameters);

        public Statement<TParams> Bind<TCallerParams>(
            ParameterConverter<TCallerParams> converter, 
            in TCallerParams parameters)
        {
            ValidateConverter(converter);
            return BindInternal(converter, in parameters);
        }

        private Statement<TParams> BindInternal<TCallerParams>(
            ParameterConverter<TCallerParams> converter, 
            in TCallerParams parameters)
        {
            // parameters are 1-indexed
            for (int i=0; i < converter.ValueBinders.Length; i++)
            {
                converter.ValueBinders[i].Bind(in parameters, _statement, i + 1);
            }
            return this;
        }

        private void ValidateConverter<TCallerParams>(ParameterConverter<TCallerParams> converter)
        {
            if (converter.ValueBinders.Length != _statement.ParameterCount)
            {
                throw new ArgumentException($"Converter expects {converter.ValueBinders.Length} parameters; query has {_statement.ParameterCount} parameters");
            }
        }

        public void Execute() => _statement.Execute();

        public void Dispose() => _statement.Dispose();
    }
}
