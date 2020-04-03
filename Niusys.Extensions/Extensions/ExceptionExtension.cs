using System;

namespace Niusys
{
    public static class ExceptionExtension
    {
        public static string FullMessage(this Exception ex)
        {
            return ex.InnerException == null
                ? ex.Message
                : string.Format("{0} -> {1}", ex.Message, ex.InnerException.FullMessage());
        }

        public static string FullStacktrace(this Exception ex)
        {
            return ex.InnerException == null
                ? ex.StackTrace
                : string.Format("{0} -> {1}", ex.StackTrace, ex.InnerException.FullStacktrace());
        }

        public static string FullExType(this Exception ex)
        {
            return ex.InnerException == null
                ? ex.GetType().FullName
                : string.Format("{0} -> {1}", ex.GetType().FullName, ex.InnerException.GetType().FullName);
        }
    }
}
