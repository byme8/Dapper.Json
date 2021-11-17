using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dapper.Json.Tests
{
    public static class Utils
    {
        public static string[] GetRegisteredJsonTypeHandlers()
        {
            var field = typeof(SqlMapper)
                .GetField("typeHandlers", BindingFlags.Static | BindingFlags.NonPublic);
            
            var handlers = field.GetValue(null) as Dictionary<Type, SqlMapper.ITypeHandler>;

            return handlers
                .Where(o => o.Key.FullName.StartsWith("Dapper.Json.Json"))
                .Select(o => o.Key.GenericTypeArguments.First().Name)
                .ToArray();
        }
        
        public static async Task<Project> ReplacePartOfDocumentAsync(this Project project, string documentName, string textToReplace, string newText)
        {
            return await project.ReplacePartsOfDocumentAsync(documentName, (textToReplace, newText));
        }
        
        public static async Task<Project> ReplacePartsOfDocumentAsync(this Project project, string documentName, params (string TextToReplace, string NewText)[] placesToReplace)
        {
            var document = project.Documents.First(o => o.Name == documentName);
            foreach (var place in placesToReplace)
            {
                var text = await document.GetTextAsync();
                var sourceText = SourceText.From(text.ToString().Replace(place.TextToReplace, place.NewText));
                document = document
                    .WithText(sourceText);
            }

            return document.Project;
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