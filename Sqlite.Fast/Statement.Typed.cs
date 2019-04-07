using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
#nullable enable
    /// <summary>
    /// Statement wraps a SQLite prepared statement that has one or more parameters and one or more result rows.
    /// Create a Statement (by calling Connection.CompileStatement), reuse it as many times as necessary, then dispose it.
    /// Statement instances are not thread-safe.
    /// </summary>
    public sealed class Statement<TResult, TParams> : IDisposable
    {
        private readonly ResultStatement<TResult> _resultStatement;
        private readonly Statement<TParams> _parameterStatement;

        internal Statement(
            ResultStatement<TResult> resultStatement,
            Statement<TParams> parameterStatement)
        {
            _resultStatement = resultStatement;
            _parameterStatement = parameterStatement;
        }

        /// <summary>
        /// Binds an instance of the parameter type to the statement's parameters.
        /// </summary>
        public Statement<TResult, TParams> Bind(in TParams parameters)
        {
            _parameterStatement.Bind(in parameters);
            return this;
        }

        /// <summary>
        /// Binds the statement's parameters using a custom converter.
        /// </summary>
        public Statement<TResult, TParams> Bind<TCallerParams>(
            ParameterConverter<TCallerParams> converter,
            in TCallerParams parameters)
        {
            _parameterStatement.Bind(converter, in parameters);
            return this;
        }

        /// <summary>
        /// Executes the statement and assigns the first result row to an instance of the result type.
        /// </summary>
        /// <returns>False if there were no result rows; true otherwise.</returns>
        public bool Execute(ref TResult result) => _resultStatement.Execute(ref result);

        /// <summary>
        /// Executes the statement and assigns the first result row using a custom converter.
        /// </summary>
        /// <returns>False if there were no result rows; true otherwise.</returns>
        public bool Execute<TCallerResult>(
            ResultConverter<TCallerResult> converter, 
            ref TCallerResult result) =>
            _resultStatement.Execute(converter, ref result);

        /// <summary>
        /// Executes the statement.
        /// </summary>
        /// <returns>An enumeration of result rows.</returns>
        public Rows<TResult> Execute() => _resultStatement.Execute();

        /// <summary>
        /// Executes the statement and assigns the result rows using a custom converter.
        /// </summary>
        /// <returns>An enueration of result rows.</returns>
        public Rows<TCallerResult> Execute<TCallerResult>(
            ResultConverter<TCallerResult> converter) =>
            _resultStatement.Execute(converter);

        /// <summary>
        /// Finalizes the prepared statement. If Dispose is not called, the prepared statement will be finalized by the finalizer thread.
        /// </summary>
        public void Dispose()
        {
            _parameterStatement.Dispose();
            _resultStatement.Dispose();
        }
    }
#nullable restore
}
