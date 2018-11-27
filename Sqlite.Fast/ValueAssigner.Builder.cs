using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public delegate T IntegerConverter<T>(long value);
    public delegate T FloatConverter<T>(double value);
    public delegate T TextConverter<T>(ReadOnlySpan<char> value);
    internal delegate T Utf8TextConverter<T>(ReadOnlySpan<byte> value); // internal until official type
    public delegate T BlobConverter<T>(ReadOnlySpan<byte> value);
    public delegate T NullConverter<T>();

    internal delegate void FieldAssigner<TRecord, TField>(ref TRecord rec, TField value);

    internal static class ValueAssigner
    {
        internal static IBuilder<TRecord> Build<TRecord>(MemberInfo member)
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
            IValueAssigner<TRecord> Compile(bool withDefaults);
            Builder<TRecord, TField> AsConcrete<TField>();
        }

        internal sealed class Builder<TRecord, TField> : IBuilder<TRecord>
        {
            public MemberInfo Member { get; }

            public IntegerConverter<TField> IntegerConverter;
            public FloatConverter<TField> FloatConverter;
            public TextConverter<TField> Utf16TextConverter;
            public Utf8TextConverter<TField> Utf8TextConverter;
            public BlobConverter<TField> BlobConverter;
            public NullConverter<TField> NullConverter;

            public Builder(MemberInfo member)
            {
                Member = member;
            }

            public IValueAssigner<TRecord> Compile(bool withDefaults)
            {
                if (withDefaults)
                {
                    IntegerConverter = IntegerConverter ?? DefaultConverters.GetIntegerConverter<TField>();
                    FloatConverter = FloatConverter ?? DefaultConverters.GetFloatConverter<TField>();
                    Utf16TextConverter = Utf16TextConverter ?? DefaultConverters.GetUtf16TextConverter<TField>();
                    Utf8TextConverter = Utf8TextConverter ?? DefaultConverters.GetUtf8TextConverter<TField>();
                    BlobConverter = BlobConverter ?? DefaultConverters.GetBlobConverter<TField>();
                    NullConverter = NullConverter ?? DefaultConverters.GetNullConverter<TField>();
                }
                return new ValueAssigner<TRecord, TField>(
                    Member.Name,
                    CompileAssigner(Member),
                    IntegerConverter,
                    FloatConverter,
                    Utf16TextConverter,
                    Utf8TextConverter,
                    BlobConverter,
                    NullConverter);
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

            public Builder<TRecord, TCallerField> AsConcrete<TCallerField>()
            {
                if (this is Builder<TRecord, TCallerField> builder)
                {
                    return builder;
                }
                throw new InvalidOperationException($"Field is {typeof(TField).Name} not {typeof(TCallerField).Name}");
            }
        }
    }
}
