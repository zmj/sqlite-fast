using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Sqlite.Fast
{
    internal interface IColumnToFieldMap<TRecord>
    {
        void AssignInteger(ref TRecord rec, long data);
        void AssignFloat(ref TRecord rec, double data);
        void AssignText(ref TRecord rec, ReadOnlySpan<char> data);
        void AssignBlob(ref TRecord rec, ReadOnlySpan<byte> data);
        void AssignNull(ref TRecord rec);
    }

    public delegate T IntegerConverter<T>(long data);
    public delegate T FloatConverter<T>(double data);
    public delegate T TextConverter<T>(ReadOnlySpan<char> data);
    public delegate T BlobConverter<T>(ReadOnlySpan<byte> data);
    public delegate T NullConverter<T>();

    public delegate void FieldAssigner<TRecord, TField>(ref TRecord rec, TField data);

    internal sealed class ColumnToFieldMap<TRecord, TField> : IColumnToFieldMap<TRecord>
    {
        private readonly IntegerConverter<TField> _convertInteger;
        private readonly FloatConverter<TField> _convertFloat;
        private readonly TextConverter<TField> _convertText;
        private readonly BlobConverter<TField> _convertBlob;
        private readonly NullConverter<TField> _convertNull;

        private readonly FieldAssigner<TRecord, TField> _assign;

        public ColumnToFieldMap(
            IntegerConverter<TField> convertInteger,
            FloatConverter<TField> convertFloat,
            TextConverter<TField> convertText,
            BlobConverter<TField> convertBlob,
            NullConverter<TField> convertNull,
            FieldAssigner<TRecord, TField> assign)
        {
            _convertInteger = convertInteger;
            _convertFloat = convertFloat;
            _convertText = convertText;
            _convertBlob = convertBlob;
            _convertNull = convertNull;

            _assign = assign;
        }

        public void AssignInteger(ref TRecord rec, long data)
        {
            if (_convertInteger == null)
            {
                throw ConversionMissingException(DataType.Integer);
            }
            _assign(ref rec, _convertInteger(data));
        }

        public void AssignFloat(ref TRecord rec, double data)
        {
            if (_convertFloat == null)
            {
                throw ConversionMissingException(DataType.Float);
            }
            _assign(ref rec, _convertFloat(data));
        }

        public void AssignText(ref TRecord rec, ReadOnlySpan<char> data)
        {
            if (_convertText == null)
            {
                throw ConversionMissingException(DataType.Text);
            }
            _assign(ref rec, _convertText(data));
        }

        public void AssignBlob(ref TRecord rec, ReadOnlySpan<byte> data)
        {
            if (_convertBlob == null)
            {
                throw ConversionMissingException(DataType.Blob);
            }
            _assign(ref rec, _convertBlob(data));
        }

        public void AssignNull(ref TRecord rec)
        {
            if (_convertNull == null)
            {
                throw ConversionMissingException(DataType.Null);
            }
            _assign(ref rec, _convertNull());
        }

        private static string FriendlyFieldType => $"{typeof(TRecord).Name}.{typeof(TField).Name}";

        private static Exception ConversionMissingException(DataType dataType)
        {
            return new ArgumentException($"Member of type {FriendlyFieldType} has no defined conversion for SQLite {dataType}");
        }
    }

    internal static class ColumnToFieldMap
    {
        internal static IBuilder<TRecord> Create<TRecord>(MemberInfo member)
        {
            var constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TRecord), GetMemberType(member) })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return (IBuilder<TRecord>)constructor.Invoke(new[] { member });
        }

        internal static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            else if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            throw new NotSupportedException(member.MemberType.ToString());
        }

        internal interface IBuilder<TRecord>
        {
            MemberInfo Member { get; }
            void SetDefaultConverters();
            IColumnToFieldMap<TRecord> Compile();
            Builder<TRecord, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TRecord, TField> : IBuilder<TRecord>
        {
            public MemberInfo Member { get; }

            private IntegerConverter<TField> _integerConverter;
            private FloatConverter<TField> _floatConverter;
            private TextConverter<TField> _textConverter;
            private BlobConverter<TField> _blobConverter;
            private NullConverter<TField> _nullConverter;

            public Builder(MemberInfo member)
            {
                Member = member;
            }

            public IColumnToFieldMap<TRecord> Compile()
            {
                return new ColumnToFieldMap<TRecord, TField>(
                    _integerConverter,
                    _floatConverter,
                    _textConverter,
                    _blobConverter,
                    _nullConverter,
                    CompileAssigner(Member));
            }

            private static FieldAssigner<TRecord, TField> CompileAssigner(MemberInfo memberInfo)
            {
                var record = Expression.Parameter(typeof(TRecord).MakeByRefType());
                Expression member;
                if (memberInfo is FieldInfo fieldInfo)
                {
                    member = Expression.Field(record, fieldInfo);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    member = Expression.Property(record, propertyInfo);
                }
                else
                {
                    throw new Exception($"Cannot assign to {memberInfo.MemberType}");
                }
                var value = Expression.Parameter(typeof(TField));
                var assignment = Expression.Assign(member, value);
                var lambda = Expression.Lambda<FieldAssigner<TRecord, TField>>(assignment, record, value);
                return lambda.Compile();
            }

            public void SetDefaultConverters()
            {
                var memberType = GetMemberType(Member);
                _integerConverter = DefaultConverters.GetIntegerConverter<TField>();
                _floatConverter = DefaultConverters.GetFloatConverter<TField>();
                _textConverter = DefaultConverters.GetTextConverter<TField>();
                _blobConverter = DefaultConverters.GetBlobConverter<TField>();
                _nullConverter = DefaultConverters.GetNullConverter<TField>();
            }

            public void SetIntegerConverter(IntegerConverter<TField> integerConverter)
            {
                _integerConverter = integerConverter;
            }

            public void SetFloatConverter(FloatConverter<TField> floatConverter)
            {
                _floatConverter = floatConverter;
            }

            public void SetTextConverter(TextConverter<TField> textConverter)
            {
                _textConverter = textConverter;
            }

            public void SetBlobConverter(BlobConverter<TField> blobConverter)
            {
                _blobConverter = blobConverter;
            }

            public void SetNullConverter(NullConverter<TField> nullConverter)
            {
                _nullConverter = nullConverter;
            }

            public Builder<TRecord, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TRecord, TCallerField> builder)
                {
                    return builder;
                }
                throw new ArgumentException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }        
    }
}
