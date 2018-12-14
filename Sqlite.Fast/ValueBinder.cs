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
        private readonly ValueBinder.Converter<TField>[] _converters;

        public ValueBinder(
            string fieldName,
            FieldGetter<TParams, TField> getter,
            IEnumerable<ValueBinder.Converter<TField>> converters)
        {
            _fieldName = fieldName;
            _getter = getter;
            if (converters is ValueBinder.Converter<TField>[] array) _converters = array;
            else _converters = converters.ToArray();
        }

        public void Bind(in TParams parameters, Statement statement, int index)
        {
            TField value = _getter(in parameters);
            ValueBinder.Converter<TField> converter = GetConverter(value);
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

        private ValueBinder.Converter<TField> GetConverter(TField value)
        {
            foreach (var converter in _converters)
            {
                if (converter.CanConvert(value))
                {
                    return converter;
                }
            }
            throw BindingException.ConversionMissing(_fieldName, typeof(TParams), value);
        }

        private void BindInteger(Statement statement, int index, TField value, ValueBinder.Converter<TField> converter)
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

        private void BindFloat(Statement statement, int index, TField value, ValueBinder.Converter<TField> converter)
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

        private void BindUtf8Text(Statement statement, int index, TField value, ValueBinder.Converter<TField> converter)
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

        private void BindUtf16Text(Statement statement, int index, TField value, ValueBinder.Converter<TField> converter)
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

        private void BindBlob(Statement statement, int index, TField value, ValueBinder.Converter<TField> converter)
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
    }
}
