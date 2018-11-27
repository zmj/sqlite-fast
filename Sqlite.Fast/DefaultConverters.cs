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
        {
            return (IntegerConverter<T>)GetIntegerConverter(typeof(T));
        }

        public static FloatConverter<T> GetFloatConverter<T>()
        {
            return (FloatConverter<T>)GetFloatConverter(typeof(T));
        }

        public static TextConverter<T> GetUtf16TextConverter<T>()
        {
            return (TextConverter<T>)GetUtf16TextConverter(typeof(T));
        }

        public static Utf8TextConverter<T> GetUtf8TextConverter<T>() 
        {
            return (Utf8TextConverter<T>)GetUtf8TextConverter(typeof(T));
        }

        public static BlobConverter<T> GetBlobConverter<T>()
        {
            return (BlobConverter<T>)GetBlobConverter(typeof(T));
        }

        public static NullConverter<T> GetNullConverter<T>()
        {
            return (NullConverter<T>)GetNullConverter(typeof(T));
        }

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
        private static IntegerConverter<bool> IntegerToBool;
        private static IntegerConverter<bool?> IntegerToBoolNull;
        private static IntegerConverter<DateTimeOffset> IntegerToDateTimeOffset;
        private static IntegerConverter<DateTimeOffset?> IntegerToDateTimeOffsetNull;

        private static Delegate GetIntegerConverter(Type type)
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
            if (type == typeof(bool)) return IntegerToBool ?? (IntegerToBool = (long value) => value != 0);
            if (type == typeof(bool?)) return IntegerToBoolNull ?? (IntegerToBoolNull = (long value) => value != 0);
            if (type == typeof(DateTimeOffset)) return IntegerToDateTimeOffset ?? 
                    (IntegerToDateTimeOffset = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
            if (type == typeof(DateTimeOffset?)) return IntegerToDateTimeOffsetNull ??
                    (IntegerToDateTimeOffsetNull = (long value) => new DateTimeOffset(ticks: value, offset: TimeSpan.Zero));
            if (type.GetTypeInfo().IsEnum)
            {
                return IntegerToEnum(type);
            }
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GenericTypeArguments[0].GetTypeInfo().IsEnum)
            {
                return IntegerToEnum(type);
            }
            return null;
        }

        private static Delegate IntegerToEnum(Type type)
        {
            var value = Expression.Parameter(typeof(long));
            return Expression.Lambda(
                typeof(IntegerConverter<>).MakeGenericType(new[] { type }),
                Expression.Convert(value, type),
                value)
                .Compile();
        }

        private static FloatConverter<double> FloatToDouble;
        private static FloatConverter<double?> FloatToDoubleNull;
        private static FloatConverter<float> FloatToFloat;
        private static FloatConverter<float?> FloatToFloatNull;
        private static FloatConverter<decimal> FloatToDecimal;
        private static FloatConverter<decimal?> FloatToDecimalNull;

        private static Delegate GetFloatConverter(Type type)
        {
            if (type == typeof(double)) return FloatToDouble ?? (FloatToDouble = (double value) => value);
            if (type == typeof(double?)) return FloatToDoubleNull ?? (FloatToDoubleNull = (double value) => value);
            if (type == typeof(float)) return FloatToFloat ?? (FloatToFloat = (double value) => (float)value);
            if (type == typeof(float?)) return FloatToFloatNull ?? (FloatToFloatNull = (double value) => (float)value);
            if (type == typeof(decimal)) return FloatToDecimal ?? (FloatToDecimal = (double value) => (decimal)value);
            if (type == typeof(decimal?)) return FloatToDecimalNull ?? (FloatToDecimalNull = (double value) => (decimal)value);
            return null;
        }

        private static TextConverter<string> TextToString;

        private static Delegate GetUtf16TextConverter(Type type)
        {
            if (type == typeof(string)) return TextToString ?? (TextToString = (ReadOnlySpan<char> text) => text.ToString());
            return null;
        }

        private static Utf8TextConverter<Guid> TextToGuid;
        private static Utf8TextConverter<Guid?> TextToGuidNull;

        private static Delegate GetUtf8TextConverter(Type type) 
        {
            if (type == typeof(Guid)) return TextToGuid ?? (TextToGuid = TextToGuidMethod);
            if (type == typeof(Guid?)) return TextToGuidNull ?? (TextToGuidNull = value => (Guid?)TextToGuidMethod(value));
            return null;
        }

        private static Guid TextToGuidMethod(ReadOnlySpan<byte> text)
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
            throw new ArgumentException($"Unable to parse guid '{Utf8ToString(text)}'");
        }

        private static unsafe string Utf8ToString(ReadOnlySpan<byte> text)
        {
            fixed (byte* b = text)
            {
                return Encoding.UTF8.GetString(b, text.Length);
            }
        }

        private static Delegate GetBlobConverter(Type type) => null;

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
        private static NullConverter<decimal?> NullToDecimalNull;
        private static NullConverter<bool?> NullToBoolNull;
        private static NullConverter<DateTimeOffset?> NullToDateTimeOffsetNull;

        private static Delegate GetNullConverter(Type type)
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
            if (type == typeof(decimal?)) return NullToDecimalNull ?? (NullToDecimalNull = () => null);
            if (type == typeof(bool?)) return NullToBoolNull ?? (NullToBoolNull = () => null);
            if (type == typeof(DateTimeOffset?)) return NullToDateTimeOffsetNull ?? (NullToDateTimeOffsetNull = () => null);
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
