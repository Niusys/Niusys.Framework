using System;

namespace Niusys.Extensions.AspNetCore.Exceptions
{
    public class ModelErrorItem
    {
        public Exception Exception { get; }
        public string ErrorMessage { get; }

        public ModelErrorItem()
        {

        }

        public ModelErrorItem(Exception exception, string errorMessage)
        {
            this.Exception = exception;
            this.ErrorMessage = errorMessage;
        }
    }
}
