using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate long ToInteger<T>(T value);
    public delegate double ToFloat<T>(T value);
    public delegate ReadOnlySpan<char> ToText<T>(T value);
    internal delegate ReadOnlySpan<byte> ToUtf8Text<T>(T value);
    public delegate ReadOnlySpan<byte> ToBlob<T>(T value);

    internal delegate TField FieldGetter<TParams, TField>(in TParams parameters);

    internal static class ValueBinder
    {
        internal static IBuilder<TParams> Build<TParams>(MemberInfo member)
        {
            var constructor = typeof(Builder<,>)
                .MakeGenericType(new[] { typeof(TParams), GetMemberType(member) })
                .GetTypeInfo()
                .GetConstructor(new[] { typeof(MemberInfo) });
            return (IBuilder<TParams>)constructor.Invoke(new[] { member });
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

        internal interface IBuilder<TParams>
        {
            MemberInfo Member { get; }
            IValueBinder<TParams> Compile(bool withDefaults);
            Builder<TParams, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TParams, TField> : IBuilder<TParams>
        {
            public MemberInfo Member { get; }
            public readonly List<Converter<TField>> Converters = new List<Converter<TField>>();

            public Builder(MemberInfo member) => Member = member;

            public IValueBinder<TParams> Compile(bool withDefaults)
            {
                IEnumerable<Converter<TField>> converters;
                if (withDefaults)
                {
                    Converter<TField>[] defaultConverters = Array.Empty<Converter<TField>>(); // todo
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
                    Member.Name,
                    CompileGetter(Member),
                    converters);
            }

            private static FieldGetter<TParams, TField> CompileGetter(MemberInfo memberInfo)
            {
                var parameters = Expression.Parameter(typeof(TParams).MakeByRefType());
                var value = Expression.MakeMemberAccess(parameters, memberInfo);
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

        internal readonly struct Converter<TField>
        {
            public readonly Sqlite.DataType DataType;
            public readonly bool Utf8Text;
            public readonly ToInteger<TField> ToInteger;
            public readonly ToFloat<TField> ToFloat;
            public readonly ToText<TField> ToUtf16Text;
            public readonly ToUtf8Text<TField> ToUtf8Text;
            public readonly ToBlob<TField> ToBlob;

            private readonly Func<TField, bool> _canConvert;

            internal Converter(
                Sqlite.DataType dataType,
                bool utf8Text,
                Func<TField, bool> canConvert,
                ToInteger<TField> toInteger,
                ToFloat<TField> toFloat,
                ToText<TField> toUtf16Text,
                ToUtf8Text<TField> toUtf8Text,
                ToBlob<TField> toBlob)
            {
                DataType = dataType;
                Utf8Text = utf8Text;
                _canConvert = canConvert;
                ToInteger = toInteger;
                ToFloat = toFloat;
                ToUtf16Text = toUtf16Text;
                ToUtf8Text = toUtf8Text;
                ToBlob = toBlob;
            }

            public bool CanConvert(TField value) => _canConvert == null || _canConvert(value);
        }

        internal static class Converter
        { 
            public static Converter<T> Integer<T>(ToInteger<T> toInteger) => Integer(canConvert: null, toInteger);
            public static Converter<T> Integer<T>(Func<T, bool> canConvert, ToInteger<T> toInteger) =>
                new Converter<T>(
                    Sqlite.DataType.Integer,
                    utf8Text: false,
                    canConvert,
                    toInteger,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);

            public static Converter<T> Float<T>(ToFloat<T> toFloat) => Float(canConvert: null, toFloat);
            public static Converter<T> Float<T>(Func<T, bool> canConvert, ToFloat<T> toFloat) =>
                new Converter<T>(
                    Sqlite.DataType.Float,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);

            public static Converter<T> Utf8Text<T>(ToUtf8Text<T> toUtf8Text) => Utf8Text(canConvert: null, toUtf8Text);
            public static Converter<T> Utf8Text<T>(Func<T, bool> canConvert, ToUtf8Text<T> toUtf8Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: true,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text,
                    toBlob: null);

            public static Converter<T> Utf16Text<T>(ToText<T> toUtf16Text) => Utf16Text(canConvert: null, toUtf16Text);
            public static Converter<T> Utf16Text<T>(Func<T, bool> canConvert, ToText<T> toUtf16Text) =>
                new Converter<T>(
                    Sqlite.DataType.Text,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text,
                    toUtf8Text: null,
                    toBlob: null);

            public static Converter<T> Blob<T>(ToBlob<T> toBlob) => Blob(canConvert: null, toBlob);
            public static Converter<T> Blob<T>(Func<T, bool> canConvert, ToBlob<T> toBlob) =>
                new Converter<T>(
                    Sqlite.DataType.Blob,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob);

            public static Converter<T> Null<T>() => Null<T>(canConvert: null);
            public static Converter<T> Null<T>(Func<T, bool> canConvert) =>
                new Converter<T>(
                    Sqlite.DataType.Null,
                    utf8Text: false,
                    canConvert,
                    toInteger: null,
                    toFloat: null,
                    toUtf16Text: null,
                    toUtf8Text: null,
                    toBlob: null);
        }
    }
}
