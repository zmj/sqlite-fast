using System;
using System.Buffers.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite.Fast
{
    internal static class DefaultConverters
    {
        public static IntegerConverter<T> GetIntegerConverter<T>()
            => (IntegerConverter<T>)Integer.GetConverter(typeof(T));

        public static FloatConverter<T> GetFloatConverter<T>()
            => (FloatConverter<T>)Float.GetConverter(typeof(T));

        public static TextConverter<T> GetUtf16TextConverter<T>()
            => (TextConverter<T>)Utf16Text.GetConverter(typeof(T));

        public static Utf8TextConverter<T> GetUtf8TextConverter<T>()
            => (Utf8TextConverter<T>)Utf8Text.GetConverter(typeof(T));

        public static BlobConverter<T> GetBlobConverter<T>()
            => (BlobConverter<T>)Blob.GetConverter(typeof(T));

        public static NullConverter<T> GetNullConverter<T>()
            => (NullConverter<T>)Null.GetConverter(typeof(T));

        private static class Integer
        {
            private static IntegerConverter<long> _toLong;
            private static IntegerConverter<long?> _toLongNull;
            private static IntegerConverter<ulong> _toUlong;
            private static IntegerConverter<ulong> _toUlongNull;
            private static IntegerConverter<int> _toInt;
            private static IntegerConverter<int?> _toIntNull;
            private static IntegerConverter<uint> _toUint;
            private static IntegerConverter<uint?> _toUintNull;
            private static IntegerConverter<short> _toShort;
            private static IntegerConverter<short?> _toShortNull;
            private static IntegerConverter<ushort> _toUshort;
            private static IntegerConverter<ushort?> _toUshortNull;
            private static IntegerConverter<char> _toChar;
            private static IntegerConverter<char?> _toCharNull;
            private static IntegerConverter<byte> _toByte;
            private static IntegerConverter<byte?> _toByteNull;
            private static IntegerConverter<sbyte> _toSbyte;
            private static IntegerConverter<sbyte?> _toSbyteNull;
            private static IntegerConverter<decimal> _toDecimal;
            private static IntegerConverter<decimal?> _toDecimalNull;
            private static IntegerConverter<bool> _toBool;
            private static IntegerConverter<bool?> _toBoolNull;
            private static IntegerConverter<DateTimeOffset> _toDateTimeOffset;
            private static IntegerConverter<DateTimeOffset?> _toDateTimeOffsetNull;
            private static IntegerConverter<TimeSpan> _toTimeSpan;
            private static IntegerConverter<TimeSpan?> _toTimeSpanNull;

            public static Delegate GetConverter(Type type)
            {
                if (type == typeof(long)) return _toLong ?? (_toLong = (long value) => value);
                if (type == typeof(long?)) return _toLongNull ?? (_toLongNull = (long value) => value);
                if (type == typeof(ulong)) return _toUlong ?? (_toUlong = (long value) => (ulong)value);
                if (type == typeof(ulong?)) return _toUlongNull ?? (_toUlongNull = (long value) => (ulong)value);
                if (type == typeof(int)) return _toInt ?? (_toInt = (long value) => (int)value);
                if (type == typeof(int?)) return _toIntNull ?? (_toIntNull = (long value) => (int)value);
                if (type == typeof(uint)) return _toUint ?? (_toUint = (long value) => (uint)value);
                if (type == typeof(uint?)) return _toUintNull ?? (_toUintNull = (long value) => (uint)value);
                if (type == typeof(short)) return _toShort ?? (_toShort = (long value) => (short)value);
                if (type == typeof(short?)) return _toShortNull ?? (_toShortNull = (long value) => (short)value);
                if (type == typeof(ushort)) return _toUshort ?? (_toUshort = (long value) => (ushort)value);
                if (type == typeof(ushort?)) return _toUshortNull ?? (_toUshortNull = (long value) => (ushort)value);
                if (type == typeof(char)) return _toChar ?? (_toChar = (long value) => (char)value);
                if (type == typeof(char?)) return _toCharNull ?? (_toCharNull = (long value) => (char)value);
                if (type == typeof(byte)) return _toByte ?? (_toByte = (long value) => (byte)value);
                if (type == typeof(byte?)) return _toByteNull ?? (_toByteNull = (long value) => (byte)value);
                if (type == typeof(sbyte)) return _toSbyte ?? (_toSbyte = (long value) => (sbyte)value);
                if (type == typeof(sbyte?)) return _toSbyteNull ?? (_toSbyteNull = (long value) => (sbyte)value);
                if (type == typeof(decimal)) return _toDecimal ?? (_toDecimal = (long value) => value);
                if (type == typeof(decimal?)) return _toDecimalNull ?? (_toDecimalNull = (long value) => value);
                if (type == typeof(bool)) return _toBool ?? (_toBool = (long value) => value != 0);
                if (type == typeof(bool?)) return _toBoolNull ?? (_toBoolNull = (long value) => value != 0);
                if (type == typeof(DateTimeOffset)) return _toDateTimeOffset ??
                        (_toDateTimeOffset = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
                if (type == typeof(DateTimeOffset?)) return _toDateTimeOffsetNull ??
                        (_toDateTimeOffsetNull = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
                if (type == typeof(TimeSpan)) return _toTimeSpan ?? (_toTimeSpan = (long value) => new TimeSpan(ticks: value));
                if (type == typeof(TimeSpan?)) return _toTimeSpanNull ?? (_toTimeSpanNull = (long value) => new TimeSpan(ticks: value));
                if (type.GetTypeInfo().IsEnum)
                {
                    return ToEnum(type);
                }
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GenericTypeArguments[0].GetTypeInfo().IsEnum)
                {
                    return ToEnum(type);
                }
                return null;
            }

            private static Delegate ToEnum(Type type)
            {
                var value = Expression.Parameter(typeof(long));
                return Expression.Lambda(
                    typeof(IntegerConverter<>).MakeGenericType(new[] { type }),
                    Expression.Convert(value, type),
                    value)
                    .Compile();
            }
        }

        private static class Float
        {
            private static FloatConverter<double> _toDouble;
            private static FloatConverter<double?> _toDoubleNull;
            private static FloatConverter<float> _toFloat;
            private static FloatConverter<float?> _toFloatNull;
            private static FloatConverter<decimal> _toDecimal;
            private static FloatConverter<decimal?> _toDecimalNull;
            
            public static Delegate GetConverter(Type type)
            {
                if (type == typeof(double)) return _toDouble ?? (_toDouble = (double value) => value);
                if (type == typeof(double?)) return _toDoubleNull ?? (_toDoubleNull = (double value) => value);
                if (type == typeof(float)) return _toFloat ?? (_toFloat = (double value) => (float)value);
                if (type == typeof(float?)) return _toFloatNull ?? (_toFloatNull = (double value) => (float)value);
                if (type == typeof(decimal)) return _toDecimal ?? (_toDecimal = (double value) => (decimal)value);
                if (type == typeof(decimal?)) return _toDecimalNull ?? (_toDecimalNull = (double value) => (decimal)value);
                return null;
            }
        }

        private static class Utf16Text
        {
            private static TextConverter<string> _toString;

            public static Delegate GetConverter(Type type)
            {
                if (type == typeof(string)) return _toString ?? (_toString = (ReadOnlySpan<char> text) => text.ToString());
                return null;
            }
        }

        private static class Utf8Text
        {
            private static Utf8TextConverter<Guid> _toGuid;
            private static Utf8TextConverter<Guid?> _toGuidNull;
            private static Utf8TextConverter<TimeSpan> _toTimeSpan;
            private static Utf8TextConverter<TimeSpan?> _toTimeSpanNull;

            public static Delegate GetConverter(Type type)
            {
                // bool
                // byte
                // dto
                // decimal
                // double
                // float
                // int
                // long
                // sbyte
                // short
                // timespan
                // uint
                // ulong
                // ushort
                if (type == typeof(Guid)) return _toGuid ?? (_toGuid = ToGuid);
                if (type == typeof(Guid?)) return _toGuidNull ?? (_toGuidNull = value => (Guid?)ToGuid(value));
                if (type == typeof(TimeSpan)) return _toTimeSpan ?? (_toTimeSpan = ToTimeSpan);
                if (type == typeof(TimeSpan?)) return _toTimeSpanNull ?? (_toTimeSpanNull = value => (TimeSpan?)ToTimeSpan(value));
                return null;
            }

            private static Guid ToGuid(ReadOnlySpan<byte> text)
            {
                char format = default;
                if (text.Length > 8)
                {
                    if (text[8] == '-') format = 'D';
                    else if (text[0] == '{') format = 'B';
                    else if (text[0] == '(') format = 'P';
                    else format = 'N';
                }
                if (Utf8Parser.TryParse(text, out Guid value, out _, format))
                {
                    return value;
                }
                return ThrowParseFailed<Guid>(text);
            }

            private static TimeSpan ToTimeSpan(ReadOnlySpan<byte> text)
            {
                if (Utf8Parser.TryParse(text, out TimeSpan value, out _))
                {
                    return value;
                }
                return ThrowParseFailed<TimeSpan>(text);
            }

            private static T ThrowParseFailed<T>(ReadOnlySpan<byte> text)
            {
                throw new ArgumentException($"Unable to parse '{Utf8ToString(text)}' to {typeof(T).Name}");
            }

            private static unsafe string Utf8ToString(ReadOnlySpan<byte> text)
            {
                fixed (byte* b = text)
                {
                    return Encoding.UTF8.GetString(b, text.Length);
                }
            }
        }

        private static class Blob
        {
            public static Delegate GetConverter(Type type) => null;
        }

        private static class Null
        {
            private static NullConverter<string> _toString;
            private static NullConverter<long?> _toLongNull;
            private static NullConverter<ulong?> _toUlongNull;
            private static NullConverter<int?> _toIntNull;
            private static NullConverter<uint?> _toUintNull;
            private static NullConverter<short?> _toShortNull;
            private static NullConverter<ushort?> _toUshortNull;
            private static NullConverter<char?> _toCharNull;
            private static NullConverter<byte?> _toByteNull;
            private static NullConverter<sbyte?> _toSbyteNull;
            private static NullConverter<decimal?> _toDecimalNull;
            private static NullConverter<bool?> _toBoolNull;
            private static NullConverter<DateTimeOffset?> _toDateTimeOffsetNull;
            private static NullConverter<TimeSpan?> _toTimeSpanNull;

            public static Delegate GetConverter(Type type)
            {
                if (type == typeof(string)) return _toString ?? (_toString = () => null);
                if (type == typeof(long?)) return _toLongNull ?? (_toLongNull = () => null);
                if (type == typeof(ulong?)) return _toUlongNull ?? (_toUlongNull = () => null);
                if (type == typeof(int?)) return _toIntNull ?? (_toIntNull = () => null);
                if (type == typeof(uint?)) return _toUintNull ?? (_toUintNull = () => null);
                if (type == typeof(short?)) return _toShortNull ?? (_toShortNull = () => null);
                if (type == typeof(ushort?)) return _toUshortNull ?? (_toUshortNull = () => null);
                if (type == typeof(char?)) return _toCharNull ?? (_toCharNull = () => null);
                if (type == typeof(byte?)) return _toByteNull ?? (_toByteNull = () => null);
                if (type == typeof(sbyte?)) return _toSbyteNull ?? (_toSbyteNull = () => null);
                if (type == typeof(decimal?)) return _toDecimalNull ?? (_toDecimalNull = () => null);
                if (type == typeof(bool?)) return _toBoolNull ?? (_toBoolNull = () => null);
                if (type == typeof(DateTimeOffset?)) return _toDateTimeOffsetNull ?? (_toDateTimeOffsetNull = () => null);
                if (type == typeof(TimeSpan?)) return _toTimeSpanNull ?? (_toTimeSpanNull = () => null);
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
