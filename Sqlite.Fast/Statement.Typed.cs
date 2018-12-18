using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TResult, TParams> : IDisposable
    {
        private readonly ResultStatement<TResult> _resultStatement;
        private readonly Statement<TParams> _parameterStatement;

        internal Statement(ResultStatement<TResult> resultStatement, Statement<TParams> parameterStatement)
        {
            _resultStatement = resultStatement;
            _parameterStatement = parameterStatement;
        }

        public Statement<TResult, TParams> Bind(in TParams parameters)
        {
            _parameterStatement.Bind(in parameters);
            return this;
        }

        public Statement<TResult, TParams> Bind<TCallerParams>(
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
