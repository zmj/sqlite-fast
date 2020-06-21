using System;
using System.Buffers.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite.Fast
{
    internal static partial class DefaultConverters
    {
        public static Func<long, T>? GetIntegerConverter<T>() =>
            (Func<long, T>?)Integer.From(typeof(T));

        public static Func<double, T>? FromFloat<T>() =>
            (Func<double, T>?)Float.From(typeof(T));

        public static FromSpan<char, T>? FromUtf16Text<T>() =>
            (FromSpan<char, T>?)Utf16Text.From(typeof(T));

        public static FromSpan<byte, T>? FromUtf8Text<T>() =>
            (FromSpan<byte, T>?)Utf8Text.From(typeof(T));

        public static FromSpan<byte, T>? FromBlob<T>() =>
            (FromSpan<byte, T>?)Blob.From(typeof(T));

        public static Func<T>? FromNull<T>() =>
            (Func<T>?)Null.From(typeof(T));

        private static class Integer
        {
            private static Func<long, long>? _toLong;
            private static Func<long, long?>? _toLongNull;
            private static Func<long, ulong>? _toUlong;
            private static Func<long, ulong>? _toUlongNull;
            private static Func<long, int>? _toInt;
            private static Func<long, int?>? _toIntNull;
            private static Func<long, uint>? _toUint;
            private static Func<long, uint?>? _toUintNull;
            private static Func<long, short>? _toShort;
            private static Func<long, short?>? _toShortNull;
            private static Func<long, ushort>? _toUshort;
            private static Func<long, ushort?>? _toUshortNull;
            private static Func<long, char>? _toChar;
            private static Func<long, char?>? _toCharNull;
            private static Func<long, byte>? _toByte;
            private static Func<long, byte?>? _toByteNull;
            private static Func<long, sbyte>? _toSbyte;
            private static Func<long, sbyte?>? _toSbyteNull;
            private static Func<long, bool>? _toBool;
            private static Func<long, bool?>? _toBoolNull;
            private static Func<long, DateTimeOffset>? _toDateTimeOffset;
            private static Func<long, DateTimeOffset?>? _toDateTimeOffsetNull;
            private static Func<long, TimeSpan>? _toTimeSpan;
            private static Func<long, TimeSpan?>? _toTimeSpanNull;
            
            public static Delegate? From(Type type)
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
                if (type == typeof(bool)) return _toBool ?? (_toBool = (long value) => value != 0);
                if (type == typeof(bool?)) return _toBoolNull ?? (_toBoolNull = (long value) => value != 0);
                if (type == typeof(DateTimeOffset)) return _toDateTimeOffset ??
                        (_toDateTimeOffset = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
                if (type == typeof(DateTimeOffset?)) return _toDateTimeOffsetNull ??
                        (_toDateTimeOffsetNull = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
                if (type == typeof(TimeSpan)) return _toTimeSpan ?? (_toTimeSpan = (long value) => new TimeSpan(ticks: value));
                if (type == typeof(TimeSpan?)) return _toTimeSpanNull ?? (_toTimeSpanNull = (long value) => new TimeSpan(ticks: value));
                if (IsEnum(type) || IsNullableEnum(type))
                {
                    return ToEnum(type);
                }
                return null;
            }

            private static Delegate ToEnum(Type type)
            {
                var value = Expression.Parameter(typeof(long));
                return Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(new[] { typeof(long), type }),
                    Expression.Convert(value, type),
                    value)
                    .Compile();
            }
        }

        private static class Float
        {
            private static Func<double, double>? _toDouble;
            private static Func<double, double?>? _toDoubleNull;
            private static Func<double, float>? _toFloat;
            private static Func<double, float?>? _toFloatNull;
            
            public static Delegate? From(Type type)
            {
                if (type == typeof(double)) return _toDouble ?? (_toDouble = (double value) => value);
                if (type == typeof(double?)) return _toDoubleNull ?? (_toDoubleNull = (double value) => value);
                if (type == typeof(float)) return _toFloat ?? (_toFloat = (double value) => (float)value);
                if (type == typeof(float?)) return _toFloatNull ?? (_toFloatNull = (double value) => (float)value);
                return null;
            }
        }

        private static class Utf16Text
        {
            private static FromSpan<char, string>? _toString;
            private static FromSpan<char, ReadOnlyMemory<char>>? _toStringMemory;

            public static Delegate? From(Type type)
            {
                if (type == typeof(string)) return _toString ?? 
                        (_toString = (ReadOnlySpan<char> text) => text.ToString());
                if (type == typeof(ReadOnlyMemory<char>)) return _toStringMemory ??
                        (_toStringMemory = (ReadOnlySpan<char> text) => text.ToString().AsMemory());
                return null;
            }
        }

        private static class Utf8Text
        {
            private static FromSpan<byte, Guid>? _toGuid;
            private static FromSpan<byte, Guid?>? _toGuidNull;
            private static FromSpan<byte, TimeSpan>? _toTimeSpan;
            private static FromSpan<byte, TimeSpan?>? _toTimeSpanNull;

            public static Delegate? From(Type type)
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
                if (type == typeof(Guid?)) return _toGuidNull ?? (_toGuidNull = (ReadOnlySpan<byte> value) => (Guid?)ToGuid(value));
                if (type == typeof(TimeSpan)) return _toTimeSpan ?? (_toTimeSpan = ToTimeSpan);
                if (type == typeof(TimeSpan?)) return _toTimeSpanNull ?? (_toTimeSpanNull = (ReadOnlySpan<byte> value) => (TimeSpan?)ToTimeSpan(value));
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
                ref byte b = ref MemoryMarshal.GetReference(text);
                fixed (byte* ptr = &b)
                {
                    return Encoding.UTF8.GetString(ptr, text.Length);
                }
            }
        }

        private static class Blob
        {
            public static Delegate? From(Type type) => null;
        }

        private static class Null
        {
            private static Func<string?>? _toString;
            private static Func<ReadOnlyMemory<char>>? _toStringMemory;
            private static Func<long?>? _toLongNull;
            private static Func<ulong?>? _toUlongNull;
            private static Func<int?>? _toIntNull;
            private static Func<uint?>? _toUintNull;
            private static Func<short?>? _toShortNull;
            private static Func<ushort?>? _toUshortNull;
            private static Func<char?>? _toCharNull;
            private static Func<byte?>? _toByteNull;
            private static Func<sbyte?>? _toSbyteNull;
            private static Func<decimal?>? _toDecimalNull;
            private static Func<bool?>? _toBoolNull;
            private static Func<DateTimeOffset?>? _toDateTimeOffsetNull;
            private static Func<TimeSpan?>? _toTimeSpanNull;

            public static Delegate? From(Type type)
            {
                if (type == typeof(string)) return _toString ?? (_toString = () => null);
                if (type == typeof(ReadOnlyMemory<char>)) return _toStringMemory ?? (_toStringMemory = () => default);
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
                if (IsClass(type))
                {
                    return Expression.Lambda(
                        typeof(Func<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                if (IsNullable(type))
                {
                    return Expression.Lambda(
                        typeof(Func<>).MakeGenericType(new[] { type }),
                        Expression.Convert(Expression.Constant(null), type))
                        .Compile();
                }
                return null;
            }
        }

        private static bool IsClass(Type type) => type.GetTypeInfo().IsClass;

        private static bool IsEnum(Type type) => type.GetTypeInfo().IsEnum;

        private static bool IsNullable(Type type) => IsNullable(type, out _);

        private static bool IsNullable(Type type, out Type? innerType) =>
            type.IsNullable(out innerType);

        private static bool IsNullableEnum(Type type) =>
            IsNullable(type, out Type? innerType) && IsEnum(innerType!);
    }
}
