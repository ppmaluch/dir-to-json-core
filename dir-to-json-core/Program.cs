using System;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json.Linq;

namespace dir_to_json_core
{

    class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication<Program>();


            #region options

            var inputDirOption = app.Option<string>("-i|--input", "(required) Input root directory to convert"
                    , CommandOptionType.SingleValue)
                .IsRequired();

            var outputOption = app.Option("-o|--output", "Output Path, if not specified the root directory of input is used"
                , CommandOptionType.SingleValue);

            var verboseOption = app.Option("-v|--verbose", "Display operation details"
                , CommandOptionType.NoValue);
            #endregion

            app.OnExecute(() =>
            {
                if (verboseOption.HasValue())
                    Console.WriteLine($"Converting {inputDirOption.ParsedValue} directory tree.");

                var inputDir = GetInputDir(inputDirOption);

                if (string.IsNullOrEmpty(inputDir))
                {
                    return;
                }

                var outputFile = outputOption.Value()
                                 ?? Path.Combine(inputDir.Substring(0, 3), @"exported_directories.json");


                if (verboseOption.HasValue())
                    Console.WriteLine(
                        $"Convert to {outputFile}");


                try
                {
                    JToken json = GetDirectory(new DirectoryInfo(@$"{inputDir}")).ToString();
                    File.WriteAllText(outputFile, json.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            app.HelpOption("-h|--help|-?");
            app.Execute(args);
        }

        private static string GetInputDir(CommandOption<string> inputOption)
        {
            var inputFile = inputOption.ParsedValue;
            if (!Path.IsPathFullyQualified(inputOption.ParsedValue))
                inputFile = Path.GetFullPath(inputOption.ParsedValue);

            if (!Directory.Exists(inputFile))
            {
                Console.WriteLine("Directory not found or not specified");
                return null;
            }

            return inputFile;
        }

        static JToken GetDirectory(DirectoryInfo directory)
        {
            return JToken.FromObject(new
            {
                directory = directory.EnumerateDirectories()
                    .ToDictionary(x => x.Name, x => GetDirectory(x)),
                file = directory.EnumerateFiles().Select(x => x.Name).ToList()
            });

        }
    }
}


