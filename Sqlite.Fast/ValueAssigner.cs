using System;

namespace Sqlite.Fast
{
    internal interface IValueAssigner<TRecord>
    {
        void AssignValue(ref TRecord record, Column value);
    }

    internal sealed class ValueAssigner<TRecord, TField> : IValueAssigner<TRecord>
    {
        private readonly string _fieldName;
        private readonly FieldAssigner<TRecord, TField> _assign;

        private readonly IntegerConverter<TField> _convertInteger;
        private readonly FloatConverter<TField> _convertFloat;
        private readonly TextConverter<TField> _convertText;
        private readonly BlobConverter<TField> _convertBlob;
        private readonly NullConverter<TField> _convertNull;

        public ValueAssigner(
            string fieldName,
            FieldAssigner<TRecord, TField> assign,
            IntegerConverter<TField> convertInteger,
            FloatConverter<TField> convertFloat,
            TextConverter<TField> convertText,
            BlobConverter<TField> convertBlob,
            NullConverter<TField> convertNull)
        {
            _fieldName = fieldName;
            _assign = assign;
            _convertInteger = convertInteger;
            _convertFloat = convertFloat;
            _convertText = convertText;
            _convertBlob = convertBlob;
            _convertNull = convertNull;
        }

        public void AssignValue(ref TRecord record, Column value)
        {
            switch (value.DataType)
            {
                case DataType.Integer:
                    AssignInteger(ref record, value.AsInteger());
                    break;
                case DataType.Float:
                    AssignFloat(ref record, value.AsFloat());
                    break;
                case DataType.Text:
                    AssignText(ref record, value.AsText());
                    break;
                case DataType.Blob:
                    AssignBlob(ref record, value.AsBlob());
                    break;
                case DataType.Null:
                    AssignNull(ref record);
                    break;
                default:
                    throw new ArgumentException($"Unexpected SQLite data type {value.DataType}");
            }
        }

        private void AssignInteger(ref TRecord rec, long value)
        {
            if (_convertInteger == null)
            {
                ThrowConversionMissing(DataType.Integer);
            }
            _assign(ref rec, _convertInteger(value));
        }

        private void AssignFloat(ref TRecord rec, double value)
        {
            if (_convertFloat == null)
            {
                ThrowConversionMissing(DataType.Float);
            }
            _assign(ref rec, _convertFloat(value));
        }

        private void AssignText(ref TRecord rec, ReadOnlySpan<char> value)
        {
            if (_convertText == null)
            {
                ThrowConversionMissing(DataType.Text);
            }
            _assign(ref rec, _convertText(value));
        }

        private void AssignBlob(ref TRecord rec, ReadOnlySpan<byte> value)
        {
            if (_convertBlob == null)
            {
                ThrowConversionMissing(DataType.Blob);
            }
            _assign(ref rec, _convertBlob(value));
        }

        private void AssignNull(ref TRecord rec)
        {
            if (_convertNull == null)
            {
                ThrowConversionMissing(DataType.Null);
            }
            _assign(ref rec, _convertNull());
        }
        
        private void ThrowConversionMissing(DataType dataType)
        {
            throw new AssignmentException(_fieldName, typeof(TField), typeof(TRecord), dataType);
        }
    }
}
