using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    internal interface IColumnToFieldMap<TRecord>
    {
        void AssignInteger(ref TRecord rec, long data);
        void AssignFloat(ref TRecord rec, double data);
        void AssignText(ref TRecord rec, ReadOnlySpan<byte> data);
        void AssignBlob(ref TRecord rec, ReadOnlySpan<byte> data);
        void AssignNull(ref TRecord rec);
    }

    public delegate T IntegerConverter<T>(long data);
    public delegate T FloatConverter<T>(double data);
    public delegate T TextConverter<T>(ReadOnlySpan<byte> data);
    public delegate T BlobConverter<T>(ReadOnlySpan<byte> data);
    public delegate T NullConverter<T>();

    public delegate void FieldAssigner<TRecord, TField>(ref TRecord rec, TField data);

    internal class ColumnToFieldMap<TRecord, TField> : IColumnToFieldMap<TRecord>
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

        public void AssignInteger(ref TRecord rec, long data) => _assign(ref rec, _convertInteger(data));
        public void AssignFloat(ref TRecord rec, double data) => _assign(ref rec, _convertFloat(data));
        public void AssignText(ref TRecord rec, ReadOnlySpan<byte> data) => _assign(ref rec, _convertText(data));
        public void AssignBlob(ref TRecord rec, ReadOnlySpan<byte> data) => _assign(ref rec, _convertBlob(data));
        public void AssignNull(ref TRecord rec) => _assign(ref rec, _convertNull());
    }

    internal static class ColumnToFieldMap
    {
        public static IBuilder<TRecord> Create<TRecord>(MemberInfo member)
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

        internal class Builder<TRecord, TField> : IBuilder<TRecord>
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
                    _integerConverter ?? ThrowIntegerConverterMissing<TField>(Member),
                    _floatConverter ?? ThrowFloatConverterMissing<TField>(Member),
                    _textConverter ?? ThrowTextConverterMissing<TField>(Member),
                    _blobConverter ?? ThrowBlobConverterMissing<TField>(Member),
                    _nullConverter ?? ThrowNullConverterMissing<TField>(Member),
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
                _integerConverter = (IntegerConverter<TField>)DefaultConverters.GetIntegerConverter(memberType);
                _floatConverter = (FloatConverter<TField>)DefaultConverters.GetFloatConverter(memberType);
                _textConverter = (TextConverter<TField>)DefaultConverters.GetTextConverter(memberType);
                _blobConverter = (BlobConverter<TField>)DefaultConverters.GetBlobConverter(memberType);
                _nullConverter = (NullConverter<TField>)DefaultConverters.GetNullConverter(memberType);
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
                _nullConverter = null;
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

        internal static string GetFriendlyName(MemberInfo member)
        {
            return $"{member.DeclaringType.Name}.{member.Name}";
        }

        internal static IntegerConverter<T> ThrowIntegerConverterMissing<T>(MemberInfo member)
        {
            return _ => throw new ArgumentException($"No integer converter defined for {GetFriendlyName(member)}");
        }

        internal static FloatConverter<T> ThrowFloatConverterMissing<T>(MemberInfo member)
        {
            return _ => throw new ArgumentException($"No float converter defined for {GetFriendlyName(member)}");
        }

        internal static TextConverter<T> ThrowTextConverterMissing<T>(MemberInfo member)
        {
            return _ => throw new ArgumentException($"No text converter defined for {GetFriendlyName(member)}");
        }

        internal static BlobConverter<T> ThrowBlobConverterMissing<T>(MemberInfo member)
        {
            return _ => throw new ArgumentException($"No blob converter defined for {GetFriendlyName(member)}");
        }

        internal static NullConverter<T> ThrowNullConverterMissing<T>(MemberInfo member)
        {
            return () => throw new ArgumentException($"No null converter defined for {GetFriendlyName(member)}");
        }

        internal static class DefaultConverters
        {
            private static IntegerConverter<long> IntegerToLong;
            private static IntegerConverter<long?> IntegerToLongNull;
            private static IntegerConverter<ulong> IntegerToUlong;
            private static IntegerConverter<ulong> IntegerToUlongNull;
            private static IntegerConverter<int> IntegerToInt;
            private static IntegerConverter<int?> IntegerToIntNull;
            private static IntegerConverter<uint> IntegerToUint;
            private static IntegerConverter<uint?> IntegerToUintNull;
            private static IntegerConverter<short> IntegerToShort;
            private static IntegerConverter<short?> IntegerToShortNull;
            private static IntegerConverter<ushort> IntegerToUshort;
            private static IntegerConverter<ushort?> IntegerToUshortNull;
            private static IntegerConverter<char> IntegerToChar;
            private static IntegerConverter<char?> IntegerToCharNull;
            private static IntegerConverter<byte> IntegerToByte;
            private static IntegerConverter<byte?> IntegerToByteNull;
            private static IntegerConverter<sbyte> IntegerToSbyte;
            private static IntegerConverter<sbyte?> IntegerToSbyteNull;
            private static IntegerConverter<decimal> IntegerToDecimal;
            private static IntegerConverter<decimal?> IntegerToDecimalNull;

            public static Delegate GetIntegerConverter(Type type)
            {
                if (type == typeof(long)) return IntegerToLong ?? (IntegerToLong = (long value) => value);
                if (type == typeof(long?)) return IntegerToLongNull ?? (IntegerToLongNull = (long value) => value);
                if (type == typeof(ulong)) return IntegerToUlong ?? (IntegerToUlong = (long value) => (ulong)value);
                if (type == typeof(ulong?)) return IntegerToUlongNull ?? (IntegerToUlongNull = (long value) => (ulong)value);
                if (type == typeof(int)) return IntegerToInt ?? (IntegerToInt = (long value) => (int)value);
                if (type == typeof(int?)) return IntegerToIntNull ?? (IntegerToIntNull = (long value) => (int)value);
                if (type == typeof(uint)) return IntegerToUint ?? (IntegerToUint = (long value) => (uint)value);
                if (type == typeof(uint?)) return IntegerToUintNull ?? (IntegerToUintNull = (long value) => (uint)value);
                if (type == typeof(short)) return IntegerToShort ?? (IntegerToShort = (long value) => (short)value);
                if (type == typeof(short?)) return IntegerToShortNull ?? (IntegerToShortNull = (long value) => (short)value);
                if (type == typeof(ushort)) return IntegerToUshort ?? (IntegerToUshort = (long value) => (ushort)value);
                if (type == typeof(ushort?)) return IntegerToUshortNull ?? (IntegerToUshortNull = (long value) => (ushort)value);
                if (type == typeof(char)) return IntegerToChar ?? (IntegerToChar = (long value) => (char)value);
                if (type == typeof(char?)) return IntegerToCharNull ?? (IntegerToCharNull = (long value) => (char)value);
                if (type == typeof(byte)) return IntegerToByte ?? (IntegerToByte = (long value) => (byte)value);
                if (type == typeof(byte?)) return IntegerToByteNull ?? (IntegerToByteNull = (long value) => (byte)value);
                if (type == typeof(sbyte)) return IntegerToSbyte ?? (IntegerToSbyte = (long value) => (sbyte)value);
                if (type == typeof(sbyte?)) return IntegerToSbyteNull ?? (IntegerToSbyteNull = (long value) => (sbyte)value);
                if (type == typeof(decimal)) return IntegerToDecimal ?? (IntegerToDecimal = (long value) => value);
                if (type == typeof(decimal?)) return IntegerToDecimalNull ?? (IntegerToDecimalNull = (long value) => value);
                // compile enum and enum
                return null;
            }

            private static FloatConverter<double> FloatToDouble;
            private static FloatConverter<double?> FloatToDoubleNull;
            private static FloatConverter<float> FloatToFloat;
            private static FloatConverter<float?> FloatToFloatNull;
            private static FloatConverter<decimal> FloatToDecimal;
            private static FloatConverter<decimal?> FloatToDecimalNull;

            public static Delegate GetFloatConverter(Type type)
            {
                if (type == typeof(double)) return FloatToDouble ?? (FloatToDouble = (double value) => value);
                if (type == typeof(double?)) return FloatToDoubleNull ?? (FloatToDoubleNull = (double value) => value);
                if (type == typeof(float)) return FloatToFloat ?? (FloatToFloat = (double value) => (float)value);
                if (type == typeof(float?)) return FloatToFloatNull ?? (FloatToFloatNull = (double value) => (float)value);
                if (type == typeof(decimal)) return FloatToDecimal ?? (FloatToDecimal = (double value) => (decimal)value);
                if (type == typeof(decimal?)) return FloatToDecimalNull ?? (FloatToDecimalNull = (double value) => (decimal)value);
                return null;
            }

            public static TextConverter<string> TextToString;

            private static string TextToStringMethod(ReadOnlySpan<byte> utf8Bytes)
            {
                if (utf8Bytes.Length == 0)
                {
                    return string.Empty;
                }
                const int maxStackAlloc = 256;
                if (utf8Bytes.Length <= maxStackAlloc)
                {
                    unsafe
                    {
                        byte* stackCopy = stackalloc byte[utf8Bytes.Length];
                        utf8Bytes.CopyTo(new Span<byte>(stackCopy, utf8Bytes.Length));
                        return Encoding.UTF8.GetString(stackCopy, utf8Bytes.Length);
                    }
                }
                byte[] heapCopy = ArrayPool<byte>.Shared.Rent(utf8Bytes.Length);
                try
                {
                    utf8Bytes.CopyTo(new Span<byte>(heapCopy));
                    return Encoding.UTF8.GetString(heapCopy, 0, utf8Bytes.Length);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(heapCopy);
                }
            }

            public static Delegate GetTextConverter(Type type)
            {
                if (type == typeof(string)) return TextToString ?? (TextToString = TextToStringMethod);
                return null;
            }

            public static Delegate GetBlobConverter(Type type) => null;

            private static NullConverter<string> NullToString;
            private static NullConverter<long?> NullToLongNull;
            private static NullConverter<ulong?> NullToUlongNull;
            private static NullConverter<int?> NullToIntNull;
            private static NullConverter<uint?> NullToUintNull;
            private static NullConverter<short?> NullToShortNull;
            private static NullConverter<ushort?> NullToUshortNull;
            private static NullConverter<char?> NullToCharNull;
            private static NullConverter<byte?> NullToByteNull;
            private static NullConverter<sbyte?> NullToSbyteNull;

            public static Delegate GetNullConverter(Type type)
            {
                if (type == typeof(string)) return NullToString ?? (NullToString = () => null);
                if (type == typeof(long?)) return NullToLongNull ?? (NullToLongNull = () => null);
                if (type == typeof(ulong?)) return NullToUlongNull ?? (NullToUlongNull = () => null);
                if (type == typeof(int?)) return NullToIntNull ?? (NullToIntNull = () => null);
                if (type == typeof(uint?)) return NullToUintNull ?? (NullToUintNull = () => null);
                if (type == typeof(short?)) return NullToShortNull ?? (NullToShortNull = () => null);
                if (type == typeof(ushort?)) return NullToUshortNull ?? (NullToUshortNull = () => null);
                if (type == typeof(char?)) return NullToCharNull ?? (NullToCharNull = () => null);
                if (type == typeof(byte?)) return NullToByteNull ?? (NullToByteNull = () => null);
                if (type == typeof(sbyte?)) return NullToSbyteNull ?? (NullToSbyteNull = () => null);
                if (type.GetTypeInfo().IsClass)
                {
                    return Expression.Lambda(
                        typeof(NullConverter<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return Expression.Lambda(
                        typeof(NullConverter<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                return null;
            }
        }        
    }
}
