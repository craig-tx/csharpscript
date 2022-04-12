using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace CSharpScript
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Check parameters
                if (args.Length == 0)
                {
                    Console.WriteLine("Please specify a C# script file");
                    Environment.Exit(-1);
                }

                // First parameter is source file path
                string scriptArg = args[0];

                //this might be a relative path. If it is, convert it to a full path for reliability
                string scriptFullPath = Path.GetFullPath(scriptArg);

                // Check file exists 
                if (!File.Exists(scriptFullPath))
                {
                    Console.WriteLine($"Specified file does not exist {scriptArg}");
                    Environment.Exit(-1);
                }

                string scriptFolder = new DirectoryInfo(Path.GetDirectoryName(scriptFullPath)).FullName;
                string source = ReadFile(scriptFullPath);
                
                // EMIT code to the final file to expose full path to script being executed
                source += "\r\n\r\npublic class ScriptGlobals" + Environment.NewLine +
                                     "{" + Environment.NewLine +
                                     $"    public static string ScriptPath = @\"{scriptFullPath}\";" + Environment.NewLine +
                                     $"    public static string ScriptFolder = @\"{scriptFolder}\";" + Environment.NewLine +
                                     "}" + Environment.NewLine;
                
                var compiler = new Compiler();
                var runner = new Runner();

                var byteCode = compiler.Compile(source, scriptFullPath);
                
                string[] parameters = new string[args.Length - 1];
                Array.Copy(args, 1, parameters, 0, args.Length - 1);

                runner.Execute(byteCode, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

        private static string ReadFile(string path)
        {
            string ret = string.Empty;
            FileInfo fi = new FileInfo(path);
            string scriptFolder = fi.Directory.FullName;
            using (StreamReader reader = File.OpenText(path))
            {
                ret = reader.ReadToEnd();
            }

            ret += "\r\n"; // ensures the Regex works below as-is to catch a final #include line with no trailing CRLF

            // now append any include file
            var matches = Regex.Matches(ret, "//#include\\s+(.*)$", RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                string includePath = match.Groups[1].Value.ToString().Replace("\r", "");
                string[] parts = Regex.Split(includePath, "[\\\\]|[/]");
                string pathForOS = string.Empty;
                foreach (var part in parts)
                {
                    pathForOS = Path.Combine(pathForOS, part);
                }
                pathForOS = Path.Combine(scriptFolder, pathForOS);
                if (!File.Exists(pathForOS))
                {
                    Console.WriteLine($"WARNING: could not find include file {pathForOS} .... the include reference was {includePath}");
                }
                else
                {
                    using (TextReader tr = new StreamReader(pathForOS))
                    {
                        string thisLine = string.Empty;
                        bool pastUsings = false;
                        while (thisLine != null)
                        {
                            thisLine = tr.ReadLine();
                            if (thisLine != null)
                            {
                                if (thisLine.IndexOf("public class", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    pastUsings = true;
                                }
                                if (pastUsings)
                                {
                                    ret += $"{thisLine}\r\n";
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }
    }
}
