using System;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.Exceptions
{
    /// <summary>
    /// General ApiException Types:
    /// 1. Request Model Validation Error: handled by ModelState.IsValid
    /// 2. System Exception while process logic
    /// 3. Custome defined api error
    /// </summary>
    public class ApiException : Exception
    {
        public int Code { get; private set; }
        public string HintMessage { get; set; }

        public ApiException(ApiStatusCode code, string hintMessage = null, string debugMessage = null)
            : base(debugMessage ?? hintMessage ?? code.GetDescription())
        {
            Code = (int)code;
            HintMessage = hintMessage ?? code.GetSuggestionHint();
        }

        public ApiException(string hintMessage, string debugMessage = "")
            : this(ApiStatusCode.GeneralError, hintMessage, debugMessage)
        {

        }
    }
}
