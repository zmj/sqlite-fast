using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class AssignmentException : Exception
    {
        public string MemberName { get; }
        public Type MemberType { get; }
        public Type RecordType { get; }
        public string DataType { get; }

        private AssignmentException(
            string memberName, 
            Type memberType, 
            Type recordType, 
            Sqlite.DataType dataType,
            string message)
            : base(message)
        {
            MemberName = memberName;
            MemberType = memberType;
            RecordType = recordType;
            DataType = dataType.ToString();
        }

        private AssignmentException(
            string memberName,
            Type memberType,
            Type recordType,
            Sqlite.DataType dataType,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            RecordType = recordType;
            DataType = dataType.ToString();
        }

        internal static AssignmentException ConversionMissing(
            string memberName,
            Type memberType,
            Type recordType,
            Sqlite.DataType dataType)
        {
            return new AssignmentException(
                memberName,
                memberType,
                recordType,
                dataType,
                $"No defined conversion from Sqlite.{dataType} to {memberType.Name} for {recordType.Name}.{memberName}. (Add custom conversions when building the {nameof(RecordConverter)}.)");
        }

        internal static AssignmentException ConversionFailed(
            string memberName,
            Type memberType,
            Type recordType,
            Sqlite.DataType dataType,
            Exception innerException)
        {
            return new AssignmentException(
                memberName,
                memberType,
                recordType,
                dataType,
                $"Conversion failed from Sqlite.{dataType} to {memberType.Name} for {recordType.Name}.{memberName}.",
                innerException);
        }
    }
}
