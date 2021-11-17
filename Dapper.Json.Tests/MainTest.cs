using System.Threading.Tasks;
using Dapper.Json.Tests.Data;
using Dapper.Json.Tests.Utils;
using Xunit;

namespace Dapper.Json.Tests;

public class MainTest
{
    [Fact]
    public async Task CompiledWithoutErrors()
    {
        var compilation = await TestProject.Project.CompileToRealAssembly();
    }
}