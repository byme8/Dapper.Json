using System.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace Dapper.Json.Tests.Data
{
    public static class TestProject
    {
        public static Project Project { get; }

        public static AdhocWorkspace Workspace { get; }

        static TestProject()
        {
            var manager = new AnalyzerManager();
            manager.GetProject(@"../../../../Dapper.Json.TestProject/Dapper.Json.TestProject.csproj");
            Workspace = manager.GetWorkspace();

            Project = Workspace.CurrentSolution.Projects.First(o => o.Name == "Dapper.Json.TestProject");
        }
    }
}