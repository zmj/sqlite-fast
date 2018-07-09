using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public class RowToRecordMap<TRecord>
    {
        internal IColumnToFieldMap<TRecord>[] ColumnMaps { get; }

        internal RowToRecordMap(IEnumerable<IColumnToFieldMap<TRecord>> columnMaps)
        {
            ColumnMaps = columnMaps.ToArray();
        }
    }

    public static class RowToRecordMap
    {
        public static Builder<TRecord> Empty<TRecord>() => new Builder<TRecord>();

        public static Builder<TRecord> Default<TRecord>()
        {
            var builder = new Builder<TRecord>();
            builder.SetDefaultMappings();
            return builder;
        }

        public class Builder<TRecord>
        {
            private readonly List<ColumnToFieldMap.IBuilder<TRecord>> _columnMapBuilders = new List<ColumnToFieldMap.IBuilder<TRecord>>();

            internal Builder()
            {
                if (typeof(TRecord).GetTypeInfo().StructLayoutAttribute.Value
                    == System.Runtime.InteropServices.LayoutKind.Auto)
                {
                    throw new ArgumentException($"Target type {typeof(TRecord).Name} must have sequential or explicit StructLayout");
                }
            }

            internal void SetDefaultMappings()
            {
                throw new NotImplementedException();
            }

            private ColumnToFieldMap.IBuilder<TRecord> GetOrAdd(MemberInfo member)
            {
                var columnMap = _columnMapBuilders.Find(cm => cm.Member == member);
                if (columnMap == null)
                {
                    columnMap = ColumnToFieldMap.Create<TRecord>(member);
                    _columnMapBuilders.Add(columnMap);
                }
                return columnMap;
            }

            private ColumnToFieldMap.Builder<TRecord, TField> GetOrAdd<TField>(Expression<Func<TRecord, TField>> propertyOrField)
            {
                return GetOrAdd(GetMemberInfo(propertyOrField)).AsConcrete<TField>();
            }

            public RowToRecordMap<TRecord> Compile()
            {
                var columnMaps = new List<IColumnToFieldMap<TRecord>>(capacity: _columnMapBuilders.Count);
                foreach (var colBuilder in _columnMapBuilders)
                {
                    IColumnToFieldMap<TRecord> colMap = colBuilder.Compile();
                    columnMaps.Add(colMap);
                }
                return new RowToRecordMap<TRecord>(columnMaps);
            }

            public Builder<TRecord> Custom<TField>(Expression<Func<TRecord, TField>> propertyOrField, IntegerConverter<TField> integerConverter)
            {
                GetOrAdd(propertyOrField).SetIntegerConverter(integerConverter);
                return this;
            }

            public Builder<TRecord> Custom<TField>(Expression<Func<TRecord, TField>> propertyOrField, FloatConverter<TField> floatConverter)
            {
                GetOrAdd(propertyOrField).SetFloatConverter(floatConverter);
                return this;
            }

            public Builder<TRecord> Custom<TField>(Expression<Func<TRecord, TField>> propertyOrField, TextConverter<TField> textConverter)
            {
                GetOrAdd(propertyOrField).SetTextConverter(textConverter);
                return this;
            }

            public Builder<TRecord> Custom<TField>(Expression<Func<TRecord, TField>> propertyOrField, BlobConverter<TField> blobConverter)
            {
                GetOrAdd(propertyOrField).SetBlobConverter(blobConverter);
                return this;
            }

            public Builder<TRecord> Custom<TField>(Expression<Func<TRecord, TField>> propertyOrField, NullConverter<TField> nullConverter)
            {
                GetOrAdd(propertyOrField).SetNullConverter(nullConverter);
                return this;
            }

            private static MemberInfo GetMemberInfo<TField>(Expression<Func<TRecord, TField>> propertyOrField)
            {
                if (propertyOrField.Body is MemberExpression member && ShouldMapMember(member.Member))
                {
                    return member.Member;
                }
                throw new ArgumentException($"Expression is not get/set-able field or property of {typeof(TRecord).Name}");
            }

            private static bool ShouldMapMember(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanRead && property.CanWrite;
                }
                else if (member is FieldInfo field)
                {
                    return !field.IsInitOnly;
                }
                return false;
            }
        }
    }
}
