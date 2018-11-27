using System;

namespace Sqlite.Fast
{
    internal interface IValueAssigner<TRecord>
    {
        void Assign(ref TRecord record, Column value);
    }

    internal sealed class ValueAssigner<TRecord, TField> : IValueAssigner<TRecord>
    {
        private readonly string _fieldName;
        private readonly FieldAssigner<TRecord, TField> _assign;

        private readonly IntegerConverter<TField> _convertInteger;
        private readonly FloatConverter<TField> _convertFloat;
        private readonly TextConverter<TField> _convertTextUtf16;
        private readonly Utf8TextConverter<TField> _convertTextUtf8;
        private readonly BlobConverter<TField> _convertBlob;
        private readonly NullConverter<TField> _convertNull;

        public ValueAssigner(
            string fieldName,
            FieldAssigner<TRecord, TField> assign,
            IntegerConverter<TField> convertInteger,
            FloatConverter<TField> convertFloat,
            TextConverter<TField> convertTextUtf16,
            Utf8TextConverter<TField> convertTextUtf8,
            BlobConverter<TField> convertBlob,
            NullConverter<TField> convertNull)
        {
            _fieldName = fieldName;
            _assign = assign;
            _convertInteger = convertInteger;
            _convertFloat = convertFloat;
            _convertTextUtf16 = convertTextUtf16;
            _convertTextUtf8 = convertTextUtf8;
            _convertBlob = convertBlob;
            _convertNull = convertNull;
        }

        public void Assign(ref TRecord record, Column col)
        {
            bool converted;
            TField value = default;
            try 
            {
                switch (col.DataType)
                {
                    case DataType.Integer:
                        converted = ConvertInteger(col, ref value);
                        break;
                    case DataType.Float:
                        converted = ConvertFloat(col, ref value);
                        break;
                    case DataType.Text:
                        converted = ConvertText(col, ref value);
                        break;
                    case DataType.Blob:
                        converted = ConvertBlob(col, ref value);
                        break;
                    case DataType.Null:
                        converted = ConvertNull(ref value);
                        break;
                    default:
                        converted = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new AssignmentException(_fieldName, typeof(TField), typeof(TRecord), col.DataType, ex);
            }
            if (!converted) 
            {
                throw new AssignmentException(_fieldName, typeof(TField), typeof(TRecord), col.DataType);
            }
            _assign(ref record, value);
        }

        private bool ConvertInteger(Column col, ref TField value)
        {
            if (_convertInteger != null)
            {
                value = _convertInteger(col.AsInteger());
                return true;
            }
            return false;
        }

        private bool ConvertFloat(Column col, ref TField value)
        {
            if (_convertFloat != null)
            {
                value = _convertFloat(col.AsFloat());
                return true;
            }
            return false;
        }

        private bool ConvertText(Column col, ref TField value)
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

        private bool ConvertBlob(Column col, ref TField value)
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
