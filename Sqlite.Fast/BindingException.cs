﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class BindingException : Exception
    {
        public string MemberName { get; }
        public Type MemberType { get; }
        public Type ParameterType { get; }
        public object Value { get; }

        private BindingException(
            string memberName,
            Type memberType,
            Type parameterType,
            object value,
            string message)
            : base(message)
        {
            MemberName = memberName;
            MemberType = memberType;
            ParameterType = parameterType;
            Value = value;
        }

        private BindingException(
            string memberName,
            Type memberType,
            Type parameterType,
            object value,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            ParameterType = parameterType;
            Value = value;
        }

        internal static BindingException ConversionMissing<T>(
            string memberName,
            Type paramsType,
            T value)
        {
            return new BindingException(
                memberName,
                typeof(T),
                paramsType,
                (object)value,
                $"No defined conversion from value {value} (type {typeof(T).Name}) for {paramsType.Name}.{memberName}");
        }

        internal static BindingException ConversionFailed<T>(
            string memberName,
            Type paramsType,
            T value,
            Exception innerException)
        {
            return new BindingException(
                memberName,
                typeof(T),
                paramsType,
                (object)value,
                $"Conversion failed from value {value} (type {typeof(T).Name}) for {paramsType.Name}.{memberName}");
        }
    }
}