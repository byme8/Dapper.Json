# JSON columns for the Dapper

[The full article](https://dev.to/byme8/making-dapper-and-json-friends-5afc)

It is a small library that simplifies JSON column handling inside the Dapper. 

You can use it like that:

``` cs

public class UserData
{
    public int Id { get; set; }
    public string Login { get; set; }
    public Json<EmailData[]> Emails { get; set; }
}

public class EmailData
{
    public string Email { get; set; }
    public EmailKind Kind { get; set; }
}

var users = await connection.QueryAsync<UserData>(@$"
    select Id,
           Login,
           (select Text as Email, Kind from Emails e where e.UserId = u.Id FOR JSON PATH) as Emails
    from Users u");
    
foreach (var user in users)
{
    Console.WriteLine($"Id: {user.Id}");
    Console.WriteLine($"Login: {user.Login}");
    foreach (var email in user.Emails.Value)
    {
        Console.WriteLine($"Email: {email.Email}");
        Console.WriteLine($"Kind: {email.Kind}");
    }
}

```

# Installation

You will need to install two packages to make it work. 
First one:

``` dotnet add package Apparatus.Dapper.Json ```

It contains core classes and a source generator(about it later).

For the second one, there are two choices right now.

``` dotnet add package Apparatus.Dapper.Json.Newtonsoft ``` or ``` dotnet add package Apparatus.Dapper.Json.System ```

As you may guess, the first one will use Newtonsoft.Json for deserialization and the second one the System.Text.Json.

Done! You are ready to go.


# How it works

By itself, the implementation is straightforward. We have a source generator that looks for `` Json<T> `` and generates a module initializer like that:

``` cs 
using System;

namespace Dapper.Json
{
    public static class DapperJsonModuleInitializer
    {
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Init()
        {
            SqlMapper.AddTypeHandler(new JsonTypeHandler<global::EmailData[]>());
        }
    }
}
```
The `` JsonTypeHandler<T> `` can be insatlled via `` Apparatus.Dapper.Json.* `` packages, or you can write your own.
Here is the sample for the `` Newtonsoft.Json ``:

``` cs 
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
```




