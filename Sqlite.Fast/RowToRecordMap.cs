﻿using System;
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
                    != System.Runtime.InteropServices.LayoutKind.Sequential)
                {
                    throw new ArgumentException($"Target type {typeof(TRecord).Name} must have sequential StructLayout");
                }
            }

            internal void SetDefaultMappings()
            {
                MemberInfo[] members = typeof(TRecord).GetTypeInfo().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                foreach (var member in members)
                {
                    if (!CanAssignMember(member))
                    {
                        continue;
                    }
                    ColumnToFieldMap.IBuilder<TRecord> columnMap = GetOrAdd(member);
                    columnMap.SetDefaultConverters();
                }
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
                if(GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not get/set-able field or property of {typeof(TRecord).Name}");
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

            public Builder<TRecord> Ignore<TField>(Expression<Func<TRecord, TField>> propertyOrField)
            {
                if (GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    _columnMapBuilders.RemoveAll(columnMap => columnMap.Member == member);
                }
                return this;
            }

            private static bool GetAssignableMember<TField>(Expression<Func<TRecord, TField>> propertyOrField, out MemberInfo member)
            {
                if (propertyOrField.Body is MemberExpression memberExpression && CanAssignMember(memberExpression.Member))
                {
                    member = memberExpression.Member;
                    return true;
                }
                member = null;
                return false;
            }

            private static bool CanAssignMember(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanWrite;
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
