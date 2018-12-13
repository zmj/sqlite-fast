using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqlite.Fast
{
    internal interface IValueBinder<TParams>
    {
        void Bind(in TParams parameters, Statement statement, int index);
    }

    internal sealed class ValueBinder<TParams, TField> : IValueBinder<TParams>
    {
        private readonly string _fieldName;
        private readonly FieldGetter<TParams, TField> _getter;
        private readonly Converter[] _converters;

        public ValueBinder(string fieldName, FieldGetter<TParams, TField> getter, IEnumerable<Converter> converters)
        {
            _fieldName = fieldName;
            _getter = getter;
            if (converters is Converter[] array) _converters = array;
            else _converters = converters.ToArray();
        }

        public void Bind(in TParams parameters, Statement statement, int index)
        {
            TField value = _getter(in parameters);
            Converter converter = GetConverter(value);
            switch (converter.DataType)
            {
                case Sqlite.DataType.Integer:
                    BindInteger(statement, index, value, converter);
                    break;
                case Sqlite.DataType.Float:
                    BindFloat(statement, index, value, converter);
                    break;
                case Sqlite.DataType.Text:
                    if (converter.Utf8Text) BindUtf8Text(statement, index, value, converter);
                    else BindUtf16Text(statement, index, value, converter);
                    break;
                case Sqlite.DataType.Blob:
                    BindBlob(statement, index, value, converter);
                    break;
                case Sqlite.DataType.Null:
                    statement.BindNull(index);
                    break;
                default:
                    throw BindingException.ConversionMissing(_fieldName, typeof(TParams), value);
            }
        }

        private Converter GetConverter(TField value)
        {
            foreach (Converter converter in _converters)
            {
                if (converter.CanConvert == null || converter.CanConvert(value))
                {
                    return converter;
                }
            }
            throw BindingException.ConversionMissing(_fieldName, typeof(TParams), value);
        }

        private void BindInteger(Statement statement, int index, TField value, Converter converter)
        {
            long bindValue;
            try
            {
                bindValue = converter.ToInteger(value);
            }
            catch (Exception ex)
            {
                throw BindingException.ConversionFailed(_fieldName, typeof(TParams), value, ex);
            }
            statement.BindInteger(index, bindValue);
        }

        private void BindFloat(Statement statement, int index, TField value, Converter converter)
        {
            double bindValue;
            try
            {
                bindValue = converter.ToFloat(value);
            }
            catch (Exception ex)
            {
                throw BindingException.ConversionFailed(_fieldName, typeof(TParams), value, ex);
            }
            statement.BindFloat(index, bindValue);
        }

        private void BindUtf8Text(Statement statement, int index, TField value, Converter converter)
        {
            ReadOnlySpan<byte> bindValue;
            try
            {
                bindValue = converter.ToUtf8Text(value);
            }
            catch (Exception ex)
            {
                throw BindingException.ConversionFailed(_fieldName, typeof(TParams), value, ex);
            }
            statement.BindUtf8Text(index, bindValue);
        }

        private void BindUtf16Text(Statement statement, int index, TField value, Converter converter)
        {
            ReadOnlySpan<char> bindValue;
            try
            {
                bindValue = converter.ToUtf16Text(value);
            }
            catch (Exception ex)
            {
                throw BindingException.ConversionFailed(_fieldName, typeof(TParams), value, ex);
            }
            statement.BindUtf16Text(index, bindValue);
        }

        private void BindBlob(Statement statement, int index, TField value, Converter converter)
        {
            ReadOnlySpan<byte> bindValue;
            try
            {
                bindValue = converter.ToBlob(value);
            }
            catch (Exception ex)
            {
                throw BindingException.ConversionFailed(_fieldName, typeof(TParams), value, ex);
            }
            statement.BindBlob(index, bindValue);
        }

        internal readonly struct Converter
        {
            public readonly Sqlite.DataType DataType;
            public readonly bool Utf8Text;
            public readonly Func<TField, bool> CanConvert;
            public readonly ToInteger<TField> ToInteger;
            public readonly ToFloat<TField> ToFloat;
            public readonly ToText<TField> ToUtf16Text;
            public readonly ToUtf8Text<TField> ToUtf8Text;
            public readonly ToBlob<TField> ToBlob;
        }
    }
}
