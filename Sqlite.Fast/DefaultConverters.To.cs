using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
	internal static partial class DefaultConverters
    {
        public static ValueBinder.Converter<T>[] To<T>() =>
            (ValueBinder.Converter<T>[]?)(To(typeof(T)) ?? Array.Empty<ValueBinder.Converter<T>>());

        private static ValueBinder.Converter<string>[] _fromString;
        private static ValueBinder.Converter<ReadOnlyMemory<char>>[] _fromStringROMemory;
        private static ValueBinder.Converter<Memory<char>>[] _fromStringMemory;

        private static ValueBinder.Converter<Guid>[] _fromGuid;
        private static ValueBinder.Converter<Guid?>[] _fromGuidNull;

        private static ValueBinder.Converter<long>[] _fromLong;
        private static ValueBinder.Converter<long?>[] _fromLongNull;
        private static ValueBinder.Converter<ulong>[] _fromUlong;
        private static ValueBinder.Converter<ulong?>[] _fromUlongNull;
        private static ValueBinder.Converter<int>[] _fromInt;
        private static ValueBinder.Converter<int?>[] _fromIntNull;
        private static ValueBinder.Converter<uint>[] _fromUint;
        private static ValueBinder.Converter<uint?>[] _fromUintNull;
        private static ValueBinder.Converter<short>[] _fromShort;
        private static ValueBinder.Converter<short?>[] _fromShortNull;
        private static ValueBinder.Converter<ushort>[] _fromUshort;
        private static ValueBinder.Converter<ushort?>[] _fromUshortNull;
        private static ValueBinder.Converter<char>[] _fromChar;
        private static ValueBinder.Converter<char?>[] _fromCharNull;
        private static ValueBinder.Converter<byte>[] _fromByte;
        private static ValueBinder.Converter<byte?>[] _fromByteNull;
        private static ValueBinder.Converter<sbyte>[] _fromSbyte;
        private static ValueBinder.Converter<sbyte?>[] _fromSbyteNull;
        private static ValueBinder.Converter<bool>[] _fromBool;
        private static ValueBinder.Converter<bool?>[] _fromBoolNull;
        private static ValueBinder.Converter<DateTimeOffset>[] _fromDateTimeOffset;
        private static ValueBinder.Converter<DateTimeOffset?>[] _fromDateTimeOffsetNull;
        private static ValueBinder.Converter<TimeSpan>[] _fromTimeSpan;
        private static ValueBinder.Converter<TimeSpan?>[] _fromTimeSpanNull;

        private static ValueBinder.Converter<float>[] _fromFloat;
        private static ValueBinder.Converter<float?>[] _fromFloatNull;
        private static ValueBinder.Converter<double>[] _fromDouble;
        private static ValueBinder.Converter<double?>[] _fromDoubleNull;

        private static object? To(Type type)
        {
            if (type == typeof(string)) return _fromString ??
                    (_fromString = new[] { ValueBinder.Converter.Utf16Text((string value) => value.AsSpan()) });
            if (type == typeof(ReadOnlyMemory<char>)) return _fromStringROMemory ??
                    (_fromStringROMemory = new[] { ValueBinder.Converter.Utf16Text((ReadOnlyMemory<char> value) => value.Span) });
            if (type == typeof(Memory<char>)) return _fromStringMemory ??
                    (_fromStringMemory = new[] { ValueBinder.Converter.Utf16Text((Memory<char> value) => value.Span) });

            if (type == typeof(Guid)) return _fromGuid ??
                    (_fromGuid = new[]
                    {
                        ValueBinder.Converter.Utf8Text(
                            (Guid value, Span<byte> dest) => { if (!Utf8Formatter.TryFormat(value, dest, out _)) throw new FormatException(); },
                            _ => 36),
                    });
            if (type == typeof(Guid?)) return _fromGuidNull ??
                    (_fromGuidNull = new[]
                    {
                        ValueBinder.Converter.Utf8Text(
                            (Guid? value) => value.HasValue,
                            (Guid? value, Span<byte> dest) => { if (!Utf8Formatter.TryFormat(value!.Value, dest, out _)) throw new FormatException(); },
                            _ => 36),
                        ValueBinder.Converter.Null<Guid?>(),
                    });

            if (type == typeof(long)) return _fromLong ??
                    (_fromLong = new[] { ValueBinder.Converter.Integer((long value) => value) });
            if (type == typeof(long?)) return _fromLongNull ??
                    (_fromLongNull = new[]
                    {
                        ValueBinder.Converter.Integer((long? value) => value.HasValue, (long? value) => value!.Value),
                        ValueBinder.Converter.Null<long?>(),
                    });
            if (type == typeof(ulong)) return _fromUlong ??
                    (_fromUlong = new[] { ValueBinder.Converter.Integer((ulong value) => (long)value) });
            if (type == typeof(ulong?)) return _fromUlongNull ??
                    (_fromUlongNull = new[]
                    {
                        ValueBinder.Converter.Integer((ulong? value) => value.HasValue, (ulong? value) => (long)value!.Value),
                        ValueBinder.Converter.Null<ulong?>(),
                    });
            if (type == typeof(int)) return _fromInt ??
                    (_fromInt = new[] { ValueBinder.Converter.Integer((int value) => value) });
            if (type == typeof(int?)) return _fromIntNull ??
                    (_fromIntNull = new[]
                    {
                        ValueBinder.Converter.Integer((int? value) => value.HasValue, (int? value) => value!.Value),
                        ValueBinder.Converter.Null<int?>(),
                    });
            if (type == typeof(uint)) return _fromUint ??
                    (_fromUint = new[] { ValueBinder.Converter.Integer((uint value) => value) });
            if (type == typeof(uint?)) return _fromUintNull ??
                    (_fromUintNull = new[]
                    {
                        ValueBinder.Converter.Integer((uint? value) => value.HasValue, (uint? value) => value!.Value),
                        ValueBinder.Converter.Null<uint?>(),
                    });
            if (type == typeof(short)) return _fromShort ??
                    (_fromShort = new[] { ValueBinder.Converter.Integer((short value) => value) });
            if (type == typeof(short?)) return _fromShortNull ??
                    (_fromShortNull = new[]
                    {
                        ValueBinder.Converter.Integer((short? value) => value.HasValue, (short? value) => value!.Value),
                        ValueBinder.Converter.Null<short?>(),
                    });
            if (type == typeof(ushort)) return _fromUshort ??
                    (_fromUshort = new[] { ValueBinder.Converter.Integer((ushort value) => value) });
            if (type == typeof(ushort?)) return _fromUshortNull ??
                    (_fromUshortNull = new[]
                    {
                        ValueBinder.Converter.Integer((ushort? value) => value.HasValue, (ushort? value) => value!.Value),
                        ValueBinder.Converter.Null<ushort?>(),
                    });
            if (type == typeof(char)) return _fromChar ??
                    (_fromChar = new[] { ValueBinder.Converter.Integer((char value) => value) });
            if (type == typeof(char?)) return _fromCharNull ??
                    (_fromCharNull = new[]
                    {
                        ValueBinder.Converter.Integer((char? value) => value.HasValue, (char? value) => value!.Value),
                        ValueBinder.Converter.Null<char?>(),
                    });
            if (type == typeof(byte)) return _fromByte ??
                    (_fromByte = new[] { ValueBinder.Converter.Integer((byte value) => value) });
            if (type == typeof(byte?)) return _fromByteNull ??
                    (_fromByteNull = new[]
                    {
                        ValueBinder.Converter.Integer((byte? value) => value.HasValue, (byte? value) => value!.Value),
                        ValueBinder.Converter.Null<byte?>(),
                    });
            if (type == typeof(sbyte)) return _fromSbyte ??
                    (_fromSbyte = new[] { ValueBinder.Converter.Integer((sbyte value) => value) });
            if (type == typeof(sbyte?)) return _fromSbyteNull ??
                    (_fromSbyteNull = new[]
                    {
                        ValueBinder.Converter.Integer((sbyte? value) => value.HasValue, (sbyte? value) => value!.Value),
                        ValueBinder.Converter.Null<sbyte?>(),
                    });
            if (type == typeof(bool)) return _fromBool ??
                    (_fromBool = new[] { ValueBinder.Converter.Integer((bool value) => value ? 1 : 0) });
            if (type == typeof(bool?)) return _fromBoolNull ??
                    (_fromBoolNull = new[]
                    {
                        ValueBinder.Converter.Integer((bool? value) => value.HasValue, (bool? value) => value!.Value ? 1 : 0),
                        ValueBinder.Converter.Null<bool?>(),
                    });
            if (type == typeof(DateTimeOffset)) return _fromDateTimeOffset ??
                    (_fromDateTimeOffset = new[] { ValueBinder.Converter.Integer((DateTimeOffset value) => value.UtcTicks) });
            if (type == typeof(DateTimeOffset?)) return _fromDateTimeOffsetNull ??
                    (_fromDateTimeOffsetNull = new[]
                    {
                        ValueBinder.Converter.Integer((DateTimeOffset? value) => value.HasValue, (DateTimeOffset? value) => value!.Value.UtcTicks),
                        ValueBinder.Converter.Null<DateTimeOffset?>(),
                    });
            if (type == typeof(TimeSpan)) return _fromTimeSpan ??
                    (_fromTimeSpan = new[] { ValueBinder.Converter.Integer((TimeSpan value) => value.Ticks) });
            if (type == typeof(TimeSpan?)) return _fromTimeSpanNull ??
                    (_fromTimeSpanNull = new[]
                    {
                        ValueBinder.Converter.Integer((TimeSpan? value) => value.HasValue, (TimeSpan? value) => value!.Value.Ticks),
                        ValueBinder.Converter.Null<TimeSpan?>(),
                    });
            if (IsEnum(type))
            {
                return InvokeGeneric(nameof(CreateFromEnum), type);
            }
            if (IsNullableEnum(type))
            {
                return InvokeGeneric(nameof(CreateFromEnumNull), type);
            }

            if (type == typeof(float)) return _fromFloat ??
                    (_fromFloat = new[] { ValueBinder.Converter.Float((float value) => value) });
            if (type == typeof(float?)) return _fromFloatNull ??
                    (_fromFloatNull = new[]
                    {
                        ValueBinder.Converter.Float((float? value) => value.HasValue, (float? value) => value!.Value),
                        ValueBinder.Converter.Null<float?>(),
                    });
            if (type == typeof(double)) return _fromDouble ??
                    (_fromDouble = new[] { ValueBinder.Converter.Float((double value) => value) });
            if (type == typeof(double?)) return _fromDoubleNull ??
                    (_fromDoubleNull = new[]
                    {
                        ValueBinder.Converter.Float((double? value) => value.HasValue, (double? value) => value!.Value),
                        ValueBinder.Converter.Null<double?>(),
                    });

            if (IsClass(type))
            {
                return InvokeGeneric(nameof(CreateFromReferenceNull), type);
            }
            if (IsNullable(type))
            {
                return InvokeGeneric(nameof(CreateFromNullableNull), type);
            }

            return null;
        }

        private static object InvokeGeneric(string methodName, Type typeParameter)
        {
            MethodInfo method = typeof(DefaultConverters).GetTypeInfo().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            return method.MakeGenericMethod(typeParameter).Invoke(obj: null, Array.Empty<object>());
        }

        private static ValueBinder.Converter<T>[] CreateFromEnum<T>()
        {
            var value = Expression.Parameter(typeof(T));
            var cast = Expression.Convert(value, typeof(long));
            var toInteger = Expression.Lambda<Func<T, long>>(cast, value).Compile();
            return new[] { ValueBinder.Converter.Integer(toInteger) };
        }

        private static ValueBinder.Converter<T>[] CreateFromEnumNull<T>()
        {
            IsNullable(typeof(T), out Type? innerType);
            Func<T, bool> hasValue;
            {
                var value = Expression.Parameter(typeof(T));
                var hasValueProperty = typeof(T).GetTypeInfo().GetProperty(nameof(Nullable<int>.HasValue));
                hasValue = Expression.Lambda<Func<T, bool>>(Expression.MakeMemberAccess(value, hasValueProperty), value).Compile();
            }
            Func<T, long> toInteger;
            {
                var value = Expression.Parameter(typeof(T));
                var valueProperty = typeof(T).GetTypeInfo().GetProperty(nameof(Nullable<int>.Value));
                var cast = Expression.Convert(Expression.MakeMemberAccess(value, valueProperty), typeof(long));
                toInteger = Expression.Lambda<Func<T, long>>(cast, value).Compile();
            }
            return new[]
            {
                ValueBinder.Converter.Integer(hasValue, toInteger),
                ValueBinder.Converter.Null<T>(),
            };
        }

        private static ValueBinder.Converter<T>[] CreateFromReferenceNull<T>()
        {
            var value = Expression.Parameter(typeof(T));
            var isNull = Expression.ReferenceEqual(value, Expression.Constant(null));
            var canConvert = Expression.Lambda<Func<T, bool>>(isNull, value).Compile();
            return new[] { ValueBinder.Converter.Null(canConvert) };
        }

        private static ValueBinder.Converter<T>[] CreateFromNullableNull<T>()
        {
            var value = Expression.Parameter(typeof(T));
            var hasValue = typeof(T).GetTypeInfo().GetProperty(nameof(Nullable<int>.HasValue));
            var isNull = Expression.Not(Expression.MakeMemberAccess(value, hasValue));
            var canConvert = Expression.Lambda<Func<T, bool>>(isNull, value).Compile();
            return new[] { ValueBinder.Converter.Null(canConvert) };
        }
    }
}
