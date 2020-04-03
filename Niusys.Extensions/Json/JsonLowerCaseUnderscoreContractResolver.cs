using Newtonsoft.Json.Serialization;
using System;
using System.Text.RegularExpressions;

namespace Niusys
{
    public class JsonLowerCaseUnderscoreContractResolver : DefaultContractResolver
    {
        private Regex _regex = new Regex("(?!(^[A-Z]))([A-Z])");

        protected override string ResolvePropertyName(string propertyName)
        {
            return _regex.Replace(propertyName, "_$2").ToLower();
        }
    }
}
