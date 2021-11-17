using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dapper.Json.Tests.Utils
{
    public static class TestExtensions
    {
        
        public static async Task<Project> ReplacePartOfDocumentAsync(this Project project, string documentName, string textToReplace, string newText)
        {
            return await project.ReplacePartsOfDocumentAsync(documentName, (textToReplace, newText));
        }
        
        public static async Task<Project> ReplacePartsOfDocumentAsync(this Project project, string documentName, params (string TextToReplace, string NewText)[] placesToReplace)
        {
            var document = project.Documents.First(o => o.Name == documentName);
            var text = await document.GetTextAsync();
            return placesToReplace
                .Aggregate(document, (acc, o) => acc.WithText(SourceText.From(text.ToString().Replace(o.TextToReplace, o.NewText))))
                .Project;
        }

        public static async Task<Assembly> CompileToRealAssembly(this Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var analyzerResults = compilation.GetDiagnostics();

            var error = compilation.GetDiagnostics().Concat(analyzerResults)
                .FirstOrDefault(o => o.Severity == DiagnosticSeverity.Error);

            if (error != null)
            {
                throw new Exception(error.GetMessage());
            }

            using (var memoryStream = new MemoryStream())
            {
                compilation.Emit(memoryStream);
                var bytes = memoryStream.ToArray();
                var assembly = Assembly.Load(bytes);

                return assembly;
            }
        }
    }
}