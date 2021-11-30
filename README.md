# Dapper.Json adds support for JSON columns in Dapper

This is small library that simplifies JSON columns handeling inside the Dapper.
What I mean under that? Let's have a look at following sample.

Let's suppose we have the next sql schema:
``` cs
User
{
    int Id
    string FirstName
    string LastName
}

Email
{
    int Id
    int UserId
    string Text
    string Kind
}

Role
{
    int Id
    string Name
}

UserRole
{
    int UserId
    int RoleId
}
```

and we need to map it to model like that:
``` cs
public class User
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public Role[] Roles { get; set; }
  public Email[] Emails { get; set; }
  
}

public record Role(string Name, DateTimeOffset CreateDate);

public record Email(string Text, string Kind);
```

If we try to fetch all data in SQL way, we will need atleast 3 separate sql queries.

The first one will bright FirstName and LastName:
``` sql
select FirstName, LastName from Users where Id = @UserId
```

The second one will get a list of roles:
``` sql
select Name from Roles r
inner join UserRoles u on u.Role = r.Id
where u.UserId = @UserId
```

The third one will get a list of emails:
``` sql
select Text, Kind from Emails where UserId = @UserId
```






