using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Converter<TRecord>
    {
        internal readonly IValueAssigner<TRecord>[] ValueAssigners;

        internal Converter(IEnumerable<IValueAssigner<TRecord>> valueAssigners)
        {
            ValueAssigners = valueAssigners.ToArray();
        }

        public sealed class Builder
        {
            private readonly List<ValueAssigner.IBuilder<TRecord>> _assignerBuilders = new List<ValueAssigner.IBuilder<TRecord>>();
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                if (typeof(TRecord).GetTypeInfo().StructLayoutAttribute.Value
                    != System.Runtime.InteropServices.LayoutKind.Sequential)
                {
                    throw new ArgumentException($"Target type {typeof(TRecord).Name} must have sequential StructLayout");
                }
                _withDefaults = withDefaultConversions;

                var members = typeof(TRecord).GetTypeInfo()
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(m => m.MetadataToken); // why does this work?
                foreach (var member in members)
                {
                    if (!CanAssignMember(member))
                    {
                        continue;
                    }
                    ValueAssigner.IBuilder<TRecord> builder = ValueAssigner.Build<TRecord>(member);
                    _assignerBuilders.Add(builder);
                }
            }

            private ValueAssigner.IBuilder<TRecord> GetOrAdd(MemberInfo member)
            {
                var builder = _assignerBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueAssigner.Build<TRecord>(member);
                    _assignerBuilders.Add(builder);
                }
                return builder;
            }

            private ValueAssigner.Builder<TRecord, TField> GetOrAdd<TField>(Expression<Func<TRecord, TField>> propertyOrField)
            {
                if (GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not get/set-able field or property of {typeof(TRecord).Name}");
            }

            public Converter<TRecord> Compile()
            {
                var assigners = new List<IValueAssigner<TRecord>>(capacity: _assignerBuilders.Count);
                foreach (var builder in _assignerBuilders)
                {
                    IValueAssigner<TRecord> assigner = builder.Compile(_withDefaults);
                    assigners.Add(assigner);
                }
                return new Converter<TRecord>(assigners);
            }

            public Builder With<TField>(Expression<Func<TRecord, TField>> propertyOrField, IntegerConverter<TField> integerConverter)
            {
                GetOrAdd(propertyOrField).IntegerConverter = integerConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TRecord, TField>> propertyOrField, FloatConverter<TField> floatConverter)
            {
                GetOrAdd(propertyOrField).FloatConverter = floatConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TRecord, TField>> propertyOrField, TextConverter<TField> textConverter)
            {
                GetOrAdd(propertyOrField).TextConverter = textConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TRecord, TField>> propertyOrField, BlobConverter<TField> blobConverter)
            {
                GetOrAdd(propertyOrField).BlobConverter = blobConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TRecord, TField>> propertyOrField, NullConverter<TField> nullConverter)
            {
                GetOrAdd(propertyOrField).NullConverter = nullConverter;
                return this;
            }

            public Builder Ignore<TField>(Expression<Func<TRecord, TField>> propertyOrField)
            {
                if (GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    _assignerBuilders.RemoveAll(builder => builder.Member == member);
                }
                return this;
            }

            private static bool GetAssignableMember<TField>(Expression<Func<TRecord, TField>> propertyOrField, out MemberInfo member)
            {
                if (propertyOrField.Body is MemberExpression memberExpression && CanAssignMember(memberExpression.Member))
                {
                    member = memberExpression.Member;
                    return true;
                }
                member = null;
                return false;
            }

            private static bool CanAssignMember(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanWrite;
                }
                else if (member is FieldInfo field)
                {
                    return !field.IsInitOnly;
                }
                return false;
            }
        }
    }

    public static class Converter
    {
        public static Converter<TRecord>.Builder Builder<TRecord>(bool withDefaultConversions = true)
            => new Converter<TRecord>.Builder(withDefaultConversions);
    }
}
