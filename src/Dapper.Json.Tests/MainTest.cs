using System.Linq;
using System.Threading.Tasks;
using Dapper.Json.Tests.Data;
using Xunit;

namespace Dapper.Json.Tests;

public class MainTest
{
    [Fact]
    public async Task CompiledWithoutErrors()
    {
        var compilation = await TestProject.Project.CompileToRealAssembly();
    }

    [Fact]
    public async Task TypeMappingForArrayOfStringIsGenerated()
    {
        var assembly = await TestProject.Project.CompileToRealAssembly();
        var method = assembly
            .GetType("Program")!
            .GetMethod("Main")!;

        method.Invoke(null, null);

        _ = Utils
            .GetRegisteredJsonTypeHandlers()
            .First(o => o == "String[]");
    }

    [Fact]
    public async Task TypeMappingForComplexTypeIsGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("public Json<string[]> Emails { get; set; }", "public Json<Role> Role { get; set; }"));
        var assembly = await newProject.CompileToRealAssembly();

        var method = assembly
            .GetType("Program")!
            .GetMethod("Main")!;

        method.Invoke(null, null);

        _ = Utils
            .GetRegisteredJsonTypeHandlers()
            .First(o => o == "Role");
    }
    
    [Fact]
    public async Task TypeMappingForManyTypesAreGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("// place for property", "public Json<Role> Role { get; set; }"));
        var assembly = await newProject.CompileToRealAssembly();

        var method = assembly
            .GetType("Program")!
            .GetMethod("Main")!;

        method.Invoke(null, null);

        var handlers = Utils
            .GetRegisteredJsonTypeHandlers();
        
        _ = handlers.First(o => o == "String[]");
        _ = handlers.First(o => o == "Role");
    }
    
    [Fact]
    public async Task TypeMappingForTuplesAreGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 1", "(int Id, Json<int[]> Ids) args = default;"));
        var assembly = await newProject.CompileToRealAssembly();

        var method = assembly
            .GetType("Program")!
            .GetMethod("Main")!;

        method.Invoke(null, null);

        var handlers = Utils
            .GetRegisteredJsonTypeHandlers();
        
        _ = handlers.First(o => o == "Int32[]");
    }
}