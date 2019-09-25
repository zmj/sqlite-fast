using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Serializes an instance of T.
    /// </summary>
    /// <typeparam name="T">The parameter member type.</typeparam>
    /// <typeparam name="TElem">The elemental type of the SQLite value.</typeparam>
    public delegate void ToSpan<T, TElem>(T value, in Span<TElem> destination);

    /// <summary>
    /// Reinterpret casts an instance of T.
    /// </summary>
    /// <typeparam name="T">The parameter member type.</typeparam>
    /// <typeparam name="TElem">The elemental type of the SQLite value.</typeparam>
    public delegate ReadOnlySpan<TElem> AsSpan<T, TElem>(T value);
    
    internal delegate TField FieldGetter<TParams, TField>(in TParams parameters);

    internal static class ValueBinder
    {
        public static IBuilder<TParams> Build<TParams>(MemberInfo member) =>
            (IBuilder<TParams>)BuildInternal<TParams>(member);

        public static Builder<TParams, TParams> Build<TParams>() =>
            (Builder<TParams, TParams>)BuildInternal<TParams>(member: null);

        private static object BuildInternal<TParams>(MemberInfo? member)
        {
            Type valueType = member != null ? member.ValueType() : typeof(TParams);
            ConstructorInfo? constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TParams), valueType })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return constructor!.Invoke(new[] { member });
        }

        public interface IBuilder<TParams>
        {
            MemberInfo? Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        public sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo? Member { get; }
            public readonly List<Converter<TField>> Converters = new List<Converter<TField>>();

            public Builder(MemberInfo? member) => Member = member;

            IValueBinder<TParams> IBuilder<TParams>.Compile(bool withDefaults) => 
                Compile(withDefaults);

            public ValueBinder<TParams, TField> Compile(bool withDefaults)
            {
                IEnumerable<Converter<TField>> converters;
                if (withDefaults)
                {
                    Converter<TField>[] defaultConverters = DefaultConverters.To<TField>();
                    if (Converters.Count > 0)
                    {
                        Converters.AddRange(defaultConverters);
                        converters = Converters;
                    }
                    else
                    {
                        converters = defaultConverters;
                    }
                }
                else
                {
                    converters = Converters;
                }
                return new ValueBinder<TParams, TField>(
                    Member?.Name,
                    CompileGetter(Member),
                    converters);
            }

            private static FieldGetter<TParams, TField> CompileGetter(MemberInfo? member)
            {
                var parameters = Expression.Parameter(typeof(TParams).MakeByRefType());
                Expression value;
                if (member == null)
                {
                    value = parameters;
                }
                else
                {
                    value = Expression.MakeMemberAccess(parameters, member);
                }
                var lambda = Expression.Lambda<FieldGetter<TParams, TField>>(value, parameters);
                return lambda.Compile();
            }

            public Builder<TParams, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TParams, TCallerField> builder)
                {
                    return builder;
                }
                throw new InvalidOperationException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }

        public readonly struct Converter<TField>
        {
            public readonly Sqlite.DataType DataType;
            public readonly bool Utf8Text;
            public readonly Func<TField, long>? ToInteger;
            public readonly Func<TField, double>? ToFloat;
            public readonly ToSpan<TField, char>? ToUtf16Text;
            public readonly AsSpan<TField, char>? AsUtf16Text;
            public readonly ToSpan<TField, byte>? ToUtf8Text;
            public readonly AsSpan<TField, byte>? AsUtf8Text;
            public readonly ToSpan<TField, byte>? ToBlob;
            public readonly AsSpan<TField, byte>? AsBlob;
            public readonly Func<TField, int>? Length;

            private readonly Func<TField, bool>? _canConvert;

            public Converter(
                Sqlite.DataType dataType,
                bool utf8Text,
                Func<TField, bool>? canConvert,
                Func<TField, int>? length,
                Func<TField, long>? toInteger,
                Func<TField, double>? toFloat,
                ToSpan<TField, char>? toUtf16Text,
                AsSpan<TField, char>? asUtf16Text,
                ToSpan<TField, byte>? toUtf8Text,
                AsSpan<TField, byte>? asUtf8Text,
                ToSpan<TField, byte>? toBlob,
                AsSpan<TField, byte>? asBlob)
            {
                DataType = dataType;
                Utf8Text = utf8Text;
                _canConvert = canConvert;
                Length = length;
                ToInteger = toInteger;
                ToFloat = toFloat;
                ToUtf16Text = toUtf16Text;
                AsUtf16Text = asUtf16Text;
                ToUtf8Text = toUtf8Text;
                AsUtf8Text = asUtf8Text;
                ToBlob = toBlob;
                AsBlob = asBlob;
            }

            public bool CanConvert(TField value) => _canConvert == null || _canConvert(value);
        }

        public static class Converter
        { 
            public static Converter<T> Integer<T>(Func<T, long> toInteger) => 
                Integer(canConvert: null, toInteger);

            public static Converter<T> Integer<T>(Func<T, bool>? canConvert, Func<T, long> toInteger) =>
                new Converter<T>(
                    Sqlite.DataType.Integer,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Float<T>(Func<T, double> toFloat) => 
                Float(canConvert: null, toFloat);

            public static Converter<T> Float<T>(Func<T, bool>? canConvert, Func<T, double> toFloat) =>
                new Converter<T>(
                    Sqlite.DataType.Float,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf8Text<T>(ToSpan<T, byte> toUtf8Text, Func<T, int> byteLength) =>
                Utf8Text(canConvert: null, toUtf8Text, byteLength);

            public static Converter<T> Utf8Text<T>(Func<T, bool>? canConvert, ToSpan<T, byte> toUtf8Text, Func<T, int> byteLength) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    byteLength,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf8Text<T>(AsSpan<T, byte> asUtf8Text) => 
                Utf8Text(canConvert: null, asUtf8Text);

            public static Converter<T> Utf8Text<T>(Func<T, bool>? canConvert, AsSpan<T, byte> asUtf8Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf16Text<T>(ToSpan<T, char> toUtf16Text, Func<T, int> utf16Length) =>
                Utf16Text(canConvert: null, toUtf16Text, utf16Length);

            public static Converter<T> Utf16Text<T>(Func<T, bool>? canConvert, ToSpan<T, char> toUtf16Text, Func<T, int> utf16Length) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    utf16Length,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Utf16Text<T>(AsSpan<T, char> asUtf16Text) => 
                Utf16Text(canConvert: null, asUtf16Text);

            public static Converter<T> Utf16Text<T>(Func<T, bool>? canConvert, AsSpan<T, char> asUtf16Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);

            public static Converter<T> Blob<T>(ToSpan<T, byte> toBlob, Func<T, int> byteLength) => 
                Blob(canConvert: null, toBlob, byteLength);

            public static Converter<T> Blob<T>(Func<T, bool>? canConvert, ToSpan<T, byte> toBlob, Func<T, int> byteLength) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    byteLength,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob,
                    asBlob: null);

            public static Converter<T> Blob<T>(AsSpan<T, byte> asBlob) =>
                Blob(canConvert: null, asBlob);

            public static Converter<T> Blob<T>(Func<T, bool>? canConvert, AsSpan<T, byte> asBlob) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob);

            public static Converter<T> Null<T>() => Null<T>(canConvert: null);
            public static Converter<T> Null<T>(Func<T, bool>? canConvert) =>
                new Converter<T>(
                    Sqlite.DataType.Null,
                    utf8Text: false,
                    canConvert,
                    length: null,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    asUtf16Text: null,
                    toUtf8Text: null,
                    asUtf8Text: null,
                    toBlob: null,
                    asBlob: null);
        }
    }
}
