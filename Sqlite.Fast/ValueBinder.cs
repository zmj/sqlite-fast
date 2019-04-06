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
        private readonly string? _fieldName;
        private readonly FieldGetter<TParams, TField> _getter;
        private readonly ValueBinder.Converter<TField>[] _converters;

        public ValueBinder(
            string? fieldName,
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
                    if (converter.Utf8Text)
                    {
                        if (converter.AsUtf8Text != null) BindAsUtf8Text(statement, index, value, converter);
                        else BindToUtf8Text(statement, index, value, converter);
                    }
                    else
                    {
                        if (converter.AsUtf16Text != null) BindAsUtf16Text(statement, index, value, converter);
                        else BindToUtf16Text(statement, index, value, converter);
                    }
                    break;
                case Sqlite.DataType.Blob:
                    if (converter.AsBlob != null) BindAsBlob(statement, index, value, converter);
                    else BindToBlob(statement, index, value, converter);
                    break;
                case Sqlite.DataType.Null:
                    statement.BindNull(index);
                    break;
                default:
                    BindingException.ThrowConversionMissing(_fieldName, typeof(TParams), value);
                    break;
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
            BindingException.ThrowConversionMissing(_fieldName, typeof(TParams), value);
            return default;
        }

        private void BindInteger(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            long bindValue = default;
            try { bindValue = converter.ToInteger!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindInteger(index, bindValue);
        }

        private void BindFloat(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            double bindValue = default;
            try { bindValue = converter.ToFloat!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindFloat(index, bindValue);
        }

        private void BindToUtf8Text(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            int length = default;
            try { length = converter.Length!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            Span<byte> bindValue = length <= 128 ? stackalloc byte[length] : new byte[length];
            try { converter.ToUtf8Text!(value, bindValue); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindUtf8Text(index, bindValue);
        }

        private void BindAsUtf8Text(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            ReadOnlySpan<byte> bindValue = default;
            try { bindValue = converter.AsUtf8Text!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindUtf8Text(index, bindValue);
        }

        private void BindToUtf16Text(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            int length = default;
            try { length = converter.Length!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            Span<char> bindValue = length <= 64 ? stackalloc char[length] : new char[length];
            try { converter.ToUtf16Text!(value, bindValue); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindUtf16Text(index, bindValue);
        }

        private void BindAsUtf16Text(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            ReadOnlySpan<char> bindValue = default;
            try { bindValue = converter.AsUtf16Text!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindUtf16Text(index, bindValue);
        }

        private void BindToBlob(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            int length = default;
            try { length = converter.Length!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            Span<byte> bindValue = length <= 128 ? stackalloc byte[length] : new byte[length];
            try { converter.ToBlob!(value, bindValue); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindBlob(index, bindValue);
        }

        private void BindAsBlob(Statement statement, int index, TField value, in ValueBinder.Converter<TField> converter)
        {
            ReadOnlySpan<byte> bindValue = default;
            try { bindValue = converter.AsBlob!(value); }
            catch (Exception ex) { ThrowBindFailed(value, ex); }
            statement.BindBlob(index, bindValue);
        }

        private void ThrowBindFailed(TField value, Exception exception) =>
            BindingException.ThrowConversionFailed(_fieldName, typeof(TParams), value, exception);
    }
}
