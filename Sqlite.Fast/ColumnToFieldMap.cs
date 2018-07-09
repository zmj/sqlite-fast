using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal interface IColumnToFieldMap<TRecord>
    {
        void AssignInteger(ref TRecord rec, long value);
        void AssignFloat(ref TRecord rec, double value);
        void AssignText(ref TRecord rec, Span<byte> value);
        void AssignBlob(ref TRecord rec, Span<byte> value);
        void AssignNull(ref TRecord rec);
    }

    public delegate T IntegerConverter<T>(long value);
    public delegate T FloatConverter<T>(double value);
    public delegate T TextConverter<T>(Span<byte> value);
    public delegate T BlobConverter<T>(Span<byte> value);
    public delegate T NullConverter<T>();

    public delegate void FieldAssigner<Record, Field>(ref Record rec, Field value);

    internal class ColumnToFieldMap<TRecord, TField> : IColumnToFieldMap<TRecord>
    {
        private readonly IntegerConverter<TField> _convertInteger;
        private readonly FloatConverter<TField> _convertFloat;
        private readonly TextConverter<TField> _convertText;
        private readonly BlobConverter<TField> _convertBlob;
        private readonly NullConverter<TField> _convertNull;

        private readonly FieldAssigner<TRecord, TField> _assign;

        public void AssignInteger(ref TRecord rec, long value) => _assign(ref rec, _convertInteger(value));
        public void AssignFloat(ref TRecord rec, double value) => _assign(ref rec, _convertFloat(value));
        public void AssignText(ref TRecord rec, Span<byte> value) => _assign(ref rec, _convertText(value));
        public void AssignBlob(ref TRecord rec, Span<byte> value) => _assign(ref rec, _convertBlob(value));        
        public void AssignNull(ref TRecord rec) => _assign(ref rec, _convertNull());
    }
}
