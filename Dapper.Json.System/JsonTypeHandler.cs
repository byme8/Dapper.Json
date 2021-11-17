using System.Data;
using System.Text.Json;

namespace Dapper.Json
{
    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<Json<T>>
    {
        public override void SetValue(IDbDataParameter parameter, Json<T> value)
        {
            parameter.Value = JsonSerializer.Serialize(value.Value, JsonSettings.Settings);
        }

        public override Json<T> Parse(object value)
        {
            if (value is string json)
            {
                return new Json<T>(JsonSerializer.Deserialize<T>(json, JsonSettings.Settings));
            }

            return new Json<T>(default);
        }
    }
}
