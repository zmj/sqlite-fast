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
        public static FromInteger<T> GetIntegerConverter<T>() =>
            (FromInteger<T>)Integer.GetConverter(typeof(T));

        public static FromFloat<T> GetFloatConverter<T>() =>
            (FromFloat<T>)Float.GetConverter(typeof(T));

        public static FromText<T> GetUtf16TextConverter<T>() =>
            (FromText<T>)Utf16Text.GetConverter(typeof(T));

        public static FromUtf8Text<T> GetUtf8TextConverter<T>() =>
            (FromUtf8Text<T>)Utf8Text.GetConverter(typeof(T));

        public static FromBlob<T> GetBlobConverter<T>() =>
            (FromBlob<T>)Blob.GetConverter(typeof(T));

        public static FromNull<T> GetNullConverter<T>() =>
            (FromNull<T>)Null.GetConverter(typeof(T));

        private static class Integer
        {
            private static FromInteger<long> _toLong;
            private static FromInteger<long?> _toLongNull;
            private static FromInteger<ulong> _toUlong;
            private static FromInteger<ulong> _toUlongNull;
            private static FromInteger<int> _toInt;
            private static FromInteger<int?> _toIntNull;
            private static FromInteger<uint> _toUint;
            private static FromInteger<uint?> _toUintNull;
            private static FromInteger<short> _toShort;
            private static FromInteger<short?> _toShortNull;
            private static FromInteger<ushort> _toUshort;
            private static FromInteger<ushort?> _toUshortNull;
            private static FromInteger<char> _toChar;
            private static FromInteger<char?> _toCharNull;
            private static FromInteger<byte> _toByte;
            private static FromInteger<byte?> _toByteNull;
            private static FromInteger<sbyte> _toSbyte;
            private static FromInteger<sbyte?> _toSbyteNull;
            private static FromInteger<decimal> _toDecimal;
            private static FromInteger<decimal?> _toDecimalNull;
            private static FromInteger<bool> _toBool;
            private static FromInteger<bool?> _toBoolNull;
            private static FromInteger<DateTimeOffset> _toDateTimeOffset;
            private static FromInteger<DateTimeOffset?> _toDateTimeOffsetNull;
            private static FromInteger<TimeSpan> _toTimeSpan;
            private static FromInteger<TimeSpan?> _toTimeSpanNull;

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
                    typeof(FromInteger<>).MakeGenericType(new[] { type }),
                    Expression.Convert(value, type),
                    value)
                    .Compile();
            }
        }

        private static class Float
        {
            private static FromFloat<double> _toDouble;
            private static FromFloat<double?> _toDoubleNull;
            private static FromFloat<float> _toFloat;
            private static FromFloat<float?> _toFloatNull;
            private static FromFloat<decimal> _toDecimal;
            private static FromFloat<decimal?> _toDecimalNull;
            
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
            private static FromText<string> _toString;

            public static Delegate GetConverter(Type type)
            {
                if (type == typeof(string)) return _toString ?? (_toString = (ReadOnlySpan<char> text) => text.ToString());
                return null;
            }
        }

        private static class Utf8Text
        {
            private static FromUtf8Text<Guid> _toGuid;
            private static FromUtf8Text<Guid?> _toGuidNull;
            private static FromUtf8Text<TimeSpan> _toTimeSpan;
            private static FromUtf8Text<TimeSpan?> _toTimeSpanNull;

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
            private static FromNull<string> _toString;
            private static FromNull<long?> _toLongNull;
            private static FromNull<ulong?> _toUlongNull;
            private static FromNull<int?> _toIntNull;
            private static FromNull<uint?> _toUintNull;
            private static FromNull<short?> _toShortNull;
            private static FromNull<ushort?> _toUshortNull;
            private static FromNull<char?> _toCharNull;
            private static FromNull<byte?> _toByteNull;
            private static FromNull<sbyte?> _toSbyteNull;
            private static FromNull<decimal?> _toDecimalNull;
            private static FromNull<bool?> _toBoolNull;
            private static FromNull<DateTimeOffset?> _toDateTimeOffsetNull;
            private static FromNull<TimeSpan?> _toTimeSpanNull;

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
                        typeof(FromNull<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return Expression.Lambda(
                        typeof(FromNull<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                return null;
            }
        }
    }
}
