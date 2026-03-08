using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Assets.Code.Utils
{
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, Settings);

        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);
    }
}
