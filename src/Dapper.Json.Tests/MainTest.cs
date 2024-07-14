using Dapper.Json.Tests.Data;
using Dapper.Json.Tests.Utils;
using Microsoft.CodeAnalysis;

namespace Dapper.Json.Tests;

public class MainTest : IAsyncLifetime
{
    [Fact]
    public async Task CompiledWithoutErrors()
    {
        var compilation = await TestProject.Project.CompileToRealAssembly();
    }

    [Fact]
    public async Task TypeMappingForArrayOfStringIsGenerated()
    {
        var handlers = await Execute(TestProject.Project);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingForComplexTypeIsGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("public Json<string[]> Emails { get; set; }", "public Json<Role> Role { get; set; }"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingForManyTypesAreGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("// place for property", "public Json<Role> Role { get; set; }"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingForTuplesAreGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("// place to replace 1", "(int Id, Json<int[]> Ids) args = default;"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }
    
    [Fact]
    public async Task TypeMappingForDictionariesAreGenerated()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync("Program.cs",
            ("public Json<string[]> Emails { get; set; }", "public Json<System.Collections.Generic.Dictionary<string,string>> Dict { get; set; }"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingForGenericsAreIgnored()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "public class Entity<T> { public Json<T> Value { get; set;} }"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingWithAsJsonExtensionIsDetected()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("// place to replace 1", "var role = new Role().AsJson();"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    [Fact]
    public async Task TypeMappingDuplicatesAsHandled()
    {
        var newProject = await TestProject.Project.ReplacePartsOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "public class Role { public string Name { get; set;} }"),
            ("public Json<string[]> Emails { get; set; }", "public Json<Role> Role { get; set; }"),
            ("// place to replace 1", "var role = new Role().AsJson();"));

        var handlers = await Execute(newProject);

        await Verify(handlers);
    }

    private static async Task<string[]> Execute(Project project)
    {
        var assembly = await project.CompileToRealAssembly();

        var method = assembly
            .GetType("Program")!
            .GetMethod("Main")!;

        method.Invoke(null, null);

        var handlers = Utils.Utils.GetRegisteredJsonTypeHandlers();
        return handlers;
    }

    public Task InitializeAsync()
    {
        SqlMapper.ResetTypeHandlers();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}