namespace Dapper.Json;

public class Json<T>(T? value)
{
    public T? Value { get; } = value;

    public static implicit operator Json<T>(T? value) => new(value);
}