namespace Dapper.Json
{
    public class Json<T>
    {
        public Json(T? value)
        {
            Value = value;
        }

        public T? Value { get; }
    }
}