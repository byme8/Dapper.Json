namespace Dapper.Json;

public static class JsonExtension
{
    public static Json<T> AsJson<T>(this T? value) => new(value);
}