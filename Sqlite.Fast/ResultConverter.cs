using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class ResultConverter<TResult>
    {
        internal readonly IValueAssigner<TResult>[] ValueAssigners;

        internal ResultConverter(IEnumerable<IValueAssigner<TResult>> valueAssigners)
        {
            ValueAssigners = valueAssigners.ToArray();
        }

        public sealed class Builder
        {
            private readonly List<ValueAssigner.IBuilder<TResult>> _assignerBuilders = new List<ValueAssigner.IBuilder<TResult>>();
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                if (typeof(TResult).GetTypeInfo().StructLayoutAttribute.Value
                    != System.Runtime.InteropServices.LayoutKind.Sequential)
                {
                    throw new ArgumentException($"Target type {typeof(TResult).Name} must have sequential StructLayout");
                }
                _withDefaults = withDefaultConversions;

                var members = typeof(TResult).GetTypeInfo()
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(m => m.MetadataToken); // why does this work?
                foreach (var member in members)
                {
                    if (!CanAssignMember(member))
                    {
                        continue;
                    }
                    ValueAssigner.IBuilder<TResult> builder = ValueAssigner.Build<TResult>(member);
                    _assignerBuilders.Add(builder);
                }
            }

            private ValueAssigner.IBuilder<TResult> GetOrAdd(MemberInfo member)
            {
                var builder = _assignerBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueAssigner.Build<TResult>(member);
                    _assignerBuilders.Add(builder);
                }
                return builder;
            }

            private ValueAssigner.Builder<TResult, TField> GetOrAdd<TField>(Expression<Func<TResult, TField>> propertyOrField)
            {
                if (GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not get/set-able field or property of {typeof(TResult).Name}");
            }

            public ResultConverter<TResult> Compile()
            {
                var assigners = new List<IValueAssigner<TResult>>(capacity: _assignerBuilders.Count);
                foreach (var builder in _assignerBuilders)
                {
                    IValueAssigner<TResult> assigner = builder.Compile(_withDefaults);
                    assigners.Add(assigner);
                }
                return new ResultConverter<TResult>(assigners);
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromInteger<TField> integerConverter)
            {
                GetOrAdd(propertyOrField).FromInteger = integerConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromFloat<TField> floatConverter)
            {
                GetOrAdd(propertyOrField).FromFloat = floatConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromText<TField> textConverter)
            {
                GetOrAdd(propertyOrField).FromUtf16Text = textConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromBlob<TField> blobConverter)
            {
                GetOrAdd(propertyOrField).FromBlob = blobConverter;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromNull<TField> nullConverter)
            {
                GetOrAdd(propertyOrField).FromNull = nullConverter;
                return this;
            }

            public Builder Ignore<TField>(Expression<Func<TResult, TField>> propertyOrField)
            {
                if (GetAssignableMember(propertyOrField, out MemberInfo member))
                {
                    _assignerBuilders.RemoveAll(builder => builder.Member == member);
                }
                return this;
            }

            private static bool GetAssignableMember<TField>(Expression<Func<TResult, TField>> propertyOrField, out MemberInfo member)
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

    public static class ResultConverter
    {
        public static ResultConverter<TResult>.Builder Builder<TResult>(bool withDefaultConversions = true)
            => new ResultConverter<TResult>.Builder(withDefaultConversions);
    }
}
