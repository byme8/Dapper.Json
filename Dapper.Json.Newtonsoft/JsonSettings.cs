using Newtonsoft.Json;

namespace Dapper.Json
{
    public static class JsonSettings
    {
        public static JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings();
    }
}