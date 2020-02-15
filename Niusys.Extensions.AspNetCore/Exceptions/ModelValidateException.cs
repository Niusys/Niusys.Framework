using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace Niusys.Extensions.AspNetCore.Exceptions
{
    /// <summary>
    /// ModelValidateException
    /// </summary>
    public class ModelValidateException : ApiException
    {
        public ModelValidateException() : base(ApiStatusCode.ModelValidateError, "传入的参数错误")
        {

        }

        public ModelValidateException(string key, ModelErrorItem modelError) : this()
        {
            ValidateMessage.Add(new KeyValuePair<string, ModelErrorItem>(key, modelError));
        }

        public ModelValidateException(string key, string message) : this()
        {
            ValidateMessage.Add(new KeyValuePair<string, ModelErrorItem>(key, new ModelErrorItem(null, message)));
        }

        public ModelValidateException(ModelStateDictionary modelState) : this()
        {
            modelState.SelectMany(m => m.Value.Errors.Select(me =>
                         new KeyValuePair<string, ModelErrorItem>(m.Key, new ModelErrorItem(me.Exception, me.ErrorMessage)))).ToList()
                         .ForEach(x => ValidateMessage.Add(x));
        }

        public List<KeyValuePair<string, ModelErrorItem>> ValidateMessage { get; set; } = new List<KeyValuePair<string, ModelErrorItem>>();

        public override string Message
        {
            get { return $"Model Validate Error, {string.Join(",", this.ValidateMessage.Select(x => $"{x.Key}: {x.Value.ErrorMessage ?? x.Value.Exception.FullMessage()}"))}"; }
        }
    }
}
