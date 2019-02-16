using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Statement wraps a SQLite prepared statement that has one or more parameters.
    /// Create a Statement (by calling Connection.CompileStatement), reuse it as many times as necessary, then dispose it.
    /// Statement instances are not thread-safe.
    /// </summary>
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

        /// <summary>
        /// Binds an instance of the parameter type to the statement's parameters.
        /// </summary>
        public Statement<TParams> Bind(in TParams parameters) =>
            BindInternal(_converter, in parameters);
        
        /// <summary>
        /// Binds the statement's parameters using a custom converter.
        /// </summary>
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

        /// <summary>
        /// Executes the statement.
        /// </summary>
        public void Execute() => _statement.Execute();

        /// <summary>
        /// Finalizes the prepared statement. If Dispose is not called, the prepared statement will be finalized by the finalizer thread.
        /// </summary>
        public void Dispose() => _statement.Dispose();
    }
}
