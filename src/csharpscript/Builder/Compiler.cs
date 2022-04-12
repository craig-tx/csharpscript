using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CSharpScript
{
    internal class Compiler
    {
        public byte[] Compile(string sourceCode, string scriptFullPath)
        {
            //// EMIT code to the final file to expose full path to script being executed
            //sourceCode += "\r\n\r\npublic class ScriptGlobals" + Environment.NewLine +
            //            "{" + Environment.NewLine +
            //            $"    public static string ScriptPath = \"{scriptFullPath}\";" + Environment.NewLine +
            //            "}" + Environment.NewLine;

            using (var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode).Emit(peStream);

                if (!result.Success)
                {
                    Console.WriteLine("Compilation done with error.");

                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        Console.Error.WriteLine($"Line #{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}");
                    }
                    return null;
                }

                peStream.Seek(0, SeekOrigin.Begin);

                return peStream.ToArray();
            }
        }

        private static void TryAddMetadataReference(List<MetadataReference> list, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var metaRef = MetadataReference.CreateFromFile(path);
                    list.Add(metaRef);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);

            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Diagnostics.Process).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Component).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location),
                
                // https://stackoverflow.com/a/58843721 was getting errors about "The type 'Object' is defined in an assembly that is not referenced. You must add a reference to assembly 'netstandard"
                
                // Mac vs Windows
                //MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\netstandard.dll"),
                //MetadataReference.CreateFromFile(@"/usr/local/share/dotnet/shared/Microsoft.NETCore.App/5.0.10/netstandard.dll"),

                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Net.Http.FormUrlEncodedContent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.Formatting).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Net.HttpStatusCode).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location)
            };
            
            // Handle Mac vs Windows
            TryAddMetadataReference(references, @"/usr/local/share/dotnet/shared/Microsoft.NETCore.App/5.0.10/netstandard.dll");
            TryAddMetadataReference(references, @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\netstandard.dll");

            // Linux
            TryAddMetadataReference(references, @"/snap/dotnet-sdk/120/shared/Microsoft.NETCore.App/5.0.5/netstandard.dll");

            // Prepass source for #pragma reference statements        
            StringReader reader = new StringReader(sourceCode);
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;
                string pattern = "\\s*#pragma\\s+reference\\s+\"(?<path>[^\"]*)\"\\s*";
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    //Console.WriteLine("Adding ref {0}", match.Groups["path"].Value);
                    //compilerParameters.ReferencedAssemblies.Add(match.Groups["path"].Value);
                    string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string dllPath = Path.Combine(folder, match.Groups["path"].Value);
                    references.Add(MetadataReference.CreateFromFile(dllPath));
                }
            }
            
            Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
                .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            return CSharpCompilation.Create("dynamic.dll",
                new[] { parsedSyntaxTree }, 
                references: references, 
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, 
                    optimizationLevel: OptimizationLevel.Debug,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }
}