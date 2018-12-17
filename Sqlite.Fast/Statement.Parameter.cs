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
            try
            {
                _statement = statement;
                _converter = converter;
                ValidateConverter(converter);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public Statement<TParams> Bind(in TParams parameters) =>
            BindInternal(_converter, in parameters);

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
            _statement.BeginBinding();
            converter.BindValues(in parameters, _statement);
            return this;
        }

        private void ValidateConverter<TCallerParams>(ParameterConverter<TCallerParams> converter)
        {
            if (converter.FieldCount != _statement.ParameterCount)
            {
                throw new ArgumentException($"Converter expects {converter.FieldCount} parameters; query has {_statement.ParameterCount} parameters");
            }
        }

        public void Execute() => _statement.Execute();

        public void Dispose() => _statement.Dispose();
    }
}
