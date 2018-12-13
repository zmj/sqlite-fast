using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class AssignmentException : Exception
    {
        public string MemberName { get; }
        public Type MemberType { get; }
        public Type ResultType { get; }
        public string DataType { get; }

        private AssignmentException(
            string memberName, 
            Type memberType, 
            Type resultType, 
            Sqlite.DataType dataType,
            string message)
            : base(message)
        {
            MemberName = memberName;
            MemberType = memberType;
            ResultType = resultType;
            DataType = dataType.ToString();
        }

        private AssignmentException(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            ResultType = resultType;
            DataType = dataType.ToString();
        }

        internal static AssignmentException ConversionMissing(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType)
        {
            return new AssignmentException(
                memberName,
                memberType,
                resultType,
                dataType,
                $"No defined conversion from Sqlite.{dataType} to {memberType.Name} for {resultType.Name}.{memberName}. (Add custom conversions when building the {nameof(ResultConverter)}.)");
        }

        internal static AssignmentException ConversionFailed(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType,
            Exception innerException)
        {
            return new AssignmentException(
                memberName,
                memberType,
                resultType,
                dataType,
                $"Conversion failed from Sqlite.{dataType} to {memberType.Name} for {resultType.Name}.{memberName}.",
                innerException);
        }
    }
}
