using System.Text.Json;

namespace Dapper.Json
{
    public static class JsonSettings
    {
        public static JsonSerializerOptions Settings { get; set; } = new JsonSerializerOptions();
    }
}
