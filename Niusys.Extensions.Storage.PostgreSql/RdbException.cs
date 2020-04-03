using System;
using System.Collections.Generic;
using System.Text;

namespace Niusys.Extensions.Storage.PostgreSql
{
    public class RdbException : Exception
    {
        public RdbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RdbException(RdbExceptionCode databaseConnectionOpenFail, Exception innerException)
            : base(ConvertExceptionCodeToMessage(databaseConnectionOpenFail), innerException)
        {
            DatabaseConnectionOpenFail = databaseConnectionOpenFail;
        }

        public RdbExceptionCode DatabaseConnectionOpenFail { get; }

        private static string ConvertExceptionCodeToMessage(RdbExceptionCode ex)
        {
            return ex.ToString();
        }
    }
}
