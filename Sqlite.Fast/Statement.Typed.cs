using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TRecord> : IDisposable
    {
        private readonly Statement _statement;
        private readonly Converter<TRecord> _converter;

        internal Statement(Statement statement, Converter<TRecord> converter)
        {
            _statement = statement;
            _converter = converter;
        }

        public Statement<TRecord> Bind(int parameterIndex, long parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public Statement<TRecord> Bind(int parameterIndex, ulong parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public Statement<TRecord> Bind(int parameterIndex, int parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public Statement<TRecord> Bind(int parameterIndex, uint parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public Statement<TRecord> Bind(int parameterIndex, string parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public Statement<TRecord> Bind(int parameterIndex, ReadOnlySpan<char> parameterValue)
        {
            _statement.Bind(parameterIndex, parameterValue);
            return this;
        }

        public bool Execute(ref TRecord record)
        {
            var rows = ExecuteInternal(_converter).GetEnumerator();
            if (!rows.MoveNext())
            {
                return false;
            }
            rows.Current.AssignTo(ref record);
            return true;
        }

        public bool Execute<TCallerRecord>(Converter<TCallerRecord> converter, ref TCallerRecord record)
        {
            ValidateConverter(converter);
            var rows = ExecuteInternal(converter).GetEnumerator();
            if (!rows.MoveNext())
            {
                return false;
            }
            rows.Current.AssignTo(ref record);
            return true;
        }

        public Rows<TRecord> Execute() => ExecuteInternal(_converter);

        public Rows<TCallerRecord> Execute<TCallerRecord>(Converter<TCallerRecord> converter)
        {
            ValidateConverter(converter);
            return ExecuteInternal(converter);
        }

        private Rows<TCallerRecord> ExecuteInternal<TCallerRecord>(Converter<TCallerRecord> converter)
        {
            Rows rows = _statement.ExecuteInternal();
            return new Rows<TCallerRecord>(rows, converter);
        }

        private void ValidateConverter<TCallerRecord>(Converter<TCallerRecord> converter)
        {
            if (converter.ColumnMaps.Length != _statement.ColumnCount)
            {
                throw new ArgumentException($"Converter expects {converter.ColumnMaps.Length} columns; statement returns {_statement.ColumnCount} columns");
            }
        }

        public void Dispose() => _statement.Dispose();
    }
}
