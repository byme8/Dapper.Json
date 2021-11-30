using System.Data;
using Newtonsoft.Json;

namespace Dapper.Json
{
    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<Json<T>>
    {
        public override void SetValue(IDbDataParameter parameter, Json<T> value)
        {
            parameter.Value = JsonConvert.SerializeObject(value.Value, JsonSettings.Settings);
        }

        public override Json<T> Parse(object value)
        {
            if (value is string json)
            {
                return new Json<T>(JsonConvert.DeserializeObject<T>(json, JsonSettings.Settings));
            }

            return new Json<T>(default);
        }
    }
}