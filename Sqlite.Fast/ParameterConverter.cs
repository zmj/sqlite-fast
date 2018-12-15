﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class ParameterConverter<TParams>
    {
        internal readonly IValueBinder<TParams>[] ValueBinders;

        internal ParameterConverter(IEnumerable<IValueBinder<TParams>> valueBinders)
        {
            ValueBinders = valueBinders.ToArray();
        }

        public sealed class Builder
        {
            private readonly List<ValueBinder.IBuilder<TParams>> _binderBuilders;
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _binderBuilders = typeof(TParams)
                    .GetOrderedMembers()
                    .Where(CanGetValue)
                    .Select(m => ValueBinder.Build<TParams>(m))
                    .ToList();
            }

            private ValueBinder.IBuilder<TParams> GetOrAdd(MemberInfo member)
            {
                var builder = _binderBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueBinder.Build<TParams>(member);
                    _binderBuilders.Add(builder);
                }
                return builder;
            }

            private ValueBinder.Builder<TParams, TField> GetOrAdd<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                if (GetGettableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not gettable field or property of {typeof(TParams).Name}");
            }

            public ParameterConverter<TParams> Compile()
            {
                return new ParameterConverter<TParams>(_binderBuilders.Select(b => b.Compile(_withDefaults)));
            }
            
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToInteger<TField> toInteger)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToInteger<TField> toInteger)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToFloat<TField> toFloat)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField, 
                Func<TField, bool> canConvert, 
                ToFloat<TField> toFloat)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToText<TField> toText)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(toText));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToText<TField> toText)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, toText));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToBlob<TField> toBlob)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(toBlob));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToBlob<TField> toBlob)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(canConvert, toBlob));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToNull<TField> toNull)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null<TField>());
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToNull<TField> toNull)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null(canConvert));
                return this;
            }

            public Builder Ignore<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                if (GetGettableMember(propertyOrField, out MemberInfo member))
                {
                    _binderBuilders.RemoveAll(builder => builder.Member == member);
                }
                return this;
            }

            private static bool GetGettableMember<TField>(Expression<Func<TParams, TField>> propertyOrField, out MemberInfo member)
            {
                if (propertyOrField.Body is MemberExpression memberExpression && CanGetValue(memberExpression.Member))
                {
                    member = memberExpression.Member;
                    return true;
                }
                member = null;
                return false;
            }

            private static bool CanGetValue(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanRead;
                }
                else if (member is FieldInfo field)
                {
                    return true;
                }
                return false;
            }
        }
    }

    public static class ParameterConverter
    {
        public static ParameterConverter<TParams>.Builder Builder<TParams>(bool withDefaultConversions = true) =>
            new ParameterConverter<TParams>.Builder(withDefaultConversions);
    }
}
