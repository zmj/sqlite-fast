using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class ResultStatement<TResult> : IDisposable
    {
        private readonly Statement _statement;
        private readonly ResultConverter<TResult> _converter;

        internal ResultStatement(Statement statement, ResultConverter<TResult> converter)
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
        
        public bool Execute(ref TResult result)
        {
            Rows<TResult>.Enumerator rows = ExecuteInternal(_converter).GetEnumerator();
            try
            {
                if (!rows.MoveNext())
                {
                    return false;
                }
                rows.Current.AssignTo(ref result);
                return true;
            }
            finally
            {
                rows.Dispose();
            }
        }

        public bool Execute<TCallerResult>(ResultConverter<TCallerResult> converter, ref TCallerResult result)
        {
            ValidateConverter(converter);
            Rows<TCallerResult>.Enumerator rows = ExecuteInternal(converter).GetEnumerator();
            try
            {
                if (!rows.MoveNext())
                {
                    return false;
                }
                rows.Current.AssignTo(ref result);
                return true;
            }
            finally
            {
                rows.Dispose();
            }
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
            if (converter.FieldCount != _statement.ColumnCount)
            {
                throw new ArgumentException($"Converter expects {converter.FieldCount} columns; statement returns {_statement.ColumnCount} columns");
            }
        }

        public void Dispose() => _statement.Dispose();
    }
}
