using System;
using Dapper.Json;

Console.WriteLine("Hello, World!");

public class User
{
    public Json<string[]> Emails { get; set; }
}