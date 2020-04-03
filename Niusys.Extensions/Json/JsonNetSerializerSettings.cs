using Newtonsoft.Json;

namespace Niusys
{
    public static class JsonNetSerializerSettings
    {
        public static JsonSerializerSettings Instance { get; set; }

        public static JsonSerializerSettings LowerCaseUnderscore
        {
            get
            {
                var instance = new JsonSerializerSettings();
                return DeconretJsonSerializerSettings(instance);
            }
        }

        public static JsonSerializerSettings DeconretJsonSerializerSettings(JsonSerializerSettings instance)
        {
            instance.NullValueHandling = NullValueHandling.Include;
            instance.DateFormatString = $"yyyy-MM-dd HH:mm:ss";
            instance.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
            instance.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return instance;
        }
    }
}
