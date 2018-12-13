using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TResult> : IDisposable
    {
        private readonly Statement _statement;
        private readonly ResultConverter<TResult> _converter;

        internal Statement(Statement statement, ResultConverter<TResult> converter)
        {
            _statement = statement;
            _converter = converter;
        }

        public Statement<TResult> Bind(long value)
        {
            _statement.Bind(value);
            return this;
        }

        public Statement<TResult> Bind(string value)
        {
            _statement.Bind(value);
            return this;
        }

        public Statement<TResult> Bind(ReadOnlySpan<char> value)
        {
            _statement.Bind(value);
            return this;
        }

        public bool Execute(ref TResult result)
        {
            var rows = ExecuteInternal(_converter).GetEnumerator();
            if (!rows.MoveNext())
            {
                return false;
            }
            rows.Current.AssignTo(ref result);
            return true;
        }

        public bool Execute<TCallerResult>(ResultConverter<TCallerResult> converter, ref TCallerResult result)
        {
            ValidateConverter(converter);
            var rows = ExecuteInternal(converter).GetEnumerator();
            if (!rows.MoveNext())
            {
                return false;
            }
            rows.Current.AssignTo(ref result);
            return true;
        }

        public Rows<TResult> Execute() => ExecuteInternal(_converter);

        public Rows<TCallerResult> Execute<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            ValidateConverter(converter);
            return ExecuteInternal(converter);
        }

        private Rows<TCallerResult> ExecuteInternal<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            Rows rows = _statement.ExecuteInternal();
            return new Rows<TCallerResult>(rows, converter);
        }

        private void ValidateConverter<TCallerResult>(ResultConverter<TCallerResult> converter)
        {
            if (converter.ValueAssigners.Length != _statement.ColumnCount)
            {
                throw new ArgumentException($"Converter expects {converter.ValueAssigners.Length} columns; statement returns {_statement.ColumnCount} columns");
            }
        }

        public void Dispose() => _statement.Dispose();
    }
}
