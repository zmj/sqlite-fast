﻿using System;

namespace Sqlite.Fast
{
    internal interface IValueAssigner<TResult>
    {
        void Assign(ref TResult result, in Column value);
    }

    internal sealed class ValueAssigner<TResult, TField> : IValueAssigner<TResult>
    {
        private readonly string _fieldName;
        private readonly FieldSetter<TResult, TField> _setter;

        private readonly Func<long, TField> _convertInteger;
        private readonly Func<double, TField> _convertFloat;
        private readonly FromSpan<char, TField> _convertTextUtf16;
        private readonly FromSpan<byte, TField> _convertTextUtf8;
        private readonly FromSpan<byte, TField> _convertBlob;
        private readonly Func<TField> _convertNull;

        public ValueAssigner(
            string fieldName,
            FieldSetter<TResult, TField> setter,
            Func<long, TField> convertInteger,
            Func<double, TField> convertFloat,
            FromSpan<char, TField> convertTextUtf16,
            FromSpan<byte, TField> convertTextUtf8,
            FromSpan<byte, TField> convertBlob,
            Func<TField> convertNull)
        {
            _fieldName = fieldName;
            _setter = setter;
            _convertInteger = convertInteger;
            _convertFloat = convertFloat;
            _convertTextUtf16 = convertTextUtf16;
            _convertTextUtf8 = convertTextUtf8;
            _convertBlob = convertBlob;
            _convertNull = convertNull;
        }

        public void Assign(ref TResult result, in Column col)
        {
            bool converted;
            TField value = default;
            try 
            {
                switch (col.DataType)
                {
                    case Sqlite.DataType.Integer:
                        converted = ConvertInteger(col, ref value);
                        break;
                    case Sqlite.DataType.Float:
                        converted = ConvertFloat(col, ref value);
                        break;
                    case Sqlite.DataType.Text:
                        converted = ConvertText(col, ref value);
                        break;
                    case Sqlite.DataType.Blob:
                        converted = ConvertBlob(col, ref value);
                        break;
                    case Sqlite.DataType.Null:
                        converted = ConvertNull(ref value);
                        break;
                    default:
                        converted = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw AssignmentException.ConversionFailed(_fieldName, typeof(TField), typeof(TResult), col.DataType, ex);
            }
            if (!converted) 
            {
                throw AssignmentException.ConversionMissing(_fieldName, typeof(TField), typeof(TResult), col.DataType);
            }
            _setter(ref result, value);
        }

        private bool ConvertInteger(in Column col, ref TField value)
        {
            if (_convertInteger != null)
            {
                value = _convertInteger(col.AsInteger());
                return true;
            }
            return false;
        }

        private bool ConvertFloat(in Column col, ref TField value)
        {
            if (_convertFloat != null)
            {
                value = _convertFloat(col.AsFloat());
                return true;
            }
            return false;
        }

        private bool ConvertText(in Column col, ref TField value)
        {
            if (_convertTextUtf8 != null)
            {
                // assume the database encoding is utf8
                value = _convertTextUtf8(col.AsUtf8Text());
                return true;
            }
            else if (_convertTextUtf16 != null)
            {
                value = _convertTextUtf16(col.AsUtf16Text());
                return true;
            }
            return false;
        }

        private bool ConvertBlob(in Column col, ref TField value)
        {
            if (_convertBlob != null)
            {
                value = _convertBlob(col.AsBlob());
                return true;
            }
            return false;
        }

        private bool ConvertNull(ref TField value)
        {
            if (_convertNull != null)
            {
                value = _convertNull();
                return true;
            }
            return false;
        }
    }
}
