using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TParams, TResult> : IDisposable
    {
        private readonly Statement<TParams> _parameterStatement;
        private readonly ResultStatement<TResult> _resultStatement;

        internal Statement(Statement<TParams> parameterStatement, ResultStatement<TResult> resultStatement)
        {
            _parameterStatement = parameterStatement;
            _resultStatement = resultStatement;
        }

        public Statement<TParams, TResult> Bind(in TParams parameters)
        {
            _parameterStatement.Bind(in parameters);
            return this;
        }

        public Statement<TParams, TResult> Bind<TCallerParams>(
            ParameterConverter<TCallerParams> converter,
            in TCallerParams parameters)
        {
            _parameterStatement.Bind(converter, in parameters);
            return this;
        }

        public bool Execute(ref TResult result) => _resultStatement.Execute(ref result);

        public bool Execute<TCallerResult>(ResultConverter<TCallerResult> converter, ref TCallerResult result) =>
            _resultStatement.Execute(converter, ref result);

        public Rows<TResult> Execute() => _resultStatement.Execute();

        public Rows<TCallerResult> Execute<TCallerResult>(ResultConverter<TCallerResult> converter) =>
            _resultStatement.Execute(converter);

        public void Dispose()
        {
            _parameterStatement.Dispose();
            _resultStatement.Dispose();
        }
    }
}
