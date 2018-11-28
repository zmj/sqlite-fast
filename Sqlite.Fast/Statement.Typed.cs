using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Statement<TRecord> : IDisposable
    {
        private readonly Statement _statement;
        private readonly RecordConverter<TRecord> _converter;

        internal Statement(Statement statement, RecordConverter<TRecord> converter)
        {
            _statement = statement;
            _converter = converter;
        }

        public Statement<TRecord> Bind(long value)
        {
            _statement.Bind(value);
            return this;
        }

        public Statement<TRecord> Bind(string value)
        {
            _statement.Bind(value);
            return this;
        }

        public Statement<TRecord> Bind(ReadOnlySpan<char> value)
        {
            _statement.Bind(value);
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

        public bool Execute<TCallerRecord>(RecordConverter<TCallerRecord> converter, ref TCallerRecord record)
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

        public Rows<TCallerRecord> Execute<TCallerRecord>(RecordConverter<TCallerRecord> converter)
        {
            ValidateConverter(converter);
            return ExecuteInternal(converter);
        }

        private Rows<TCallerRecord> ExecuteInternal<TCallerRecord>(RecordConverter<TCallerRecord> converter)
        {
            Rows rows = _statement.ExecuteInternal();
            return new Rows<TCallerRecord>(rows, converter);
        }

        private void ValidateConverter<TCallerRecord>(RecordConverter<TCallerRecord> converter)
        {
            if (converter.ValueAssigners.Length != _statement.ColumnCount)
            {
                throw new ArgumentException($"Converter expects {converter.ValueAssigners.Length} columns; statement returns {_statement.ColumnCount} columns");
            }
        }

        public void Dispose() => _statement.Dispose();
    }
}
