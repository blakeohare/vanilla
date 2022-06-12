using System;
using System.Collections.Generic;
using System.Linq;
using Vanilla.Transpiler;

namespace Vanilla
{
    internal class Program
    {
        static void Main(string[] args)
        {
            args = GetEffectiveArgs() ?? args;

#if DEBUG
            RunMain(args);
#else
            try
            {
                RunMain(args);
            }
            catch (BuildException be)
            {
                System.Console.WriteLine(be.Message);
            }
#endif
        }

        private static void RunMain(string[] args)
        {
            Dictionary<string, string> options = ParseArgs(args);
            string sourceEntryFile = options["source"];

            ParseBundle bundle = new Parser().Parse(sourceEntryFile);
            AbstractTranspiler transpiler;
            switch (options["language"].ToLowerInvariant())
            {
                case "c": transpiler = new CTranspiler(bundle); break;
                //case "js": transpiler = new JavaScriptTranspiler(bundle); break;
                default: throw new BuildException("Unknown language: " + options["language"]);
            }

            string destDir = FileUtil.NormalizeToAbsolute(options["destination"]);
            FileUtil.EnsureDirectoryExists(destDir);
            if (!System.IO.Directory.Exists(destDir))
            {
                throw new BuildException("Destination directory does not exist: " + destDir);
            }

            transpiler.EmitFiles(destDir);
        }

        private static Dictionary<string, string> ParseArgs(string[] rawArgs)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string entryFile = null;
            string language = null;
            string outputPath = null;

            for (int i = 0; i < rawArgs.Length; i++)
            {
                string arg = rawArgs[i].Trim();
                if (arg.StartsWith("--") && arg.Contains(':'))
                {
                    string[] pieces = arg.Split(':');
                    string key = pieces[0].Substring(2).ToLowerInvariant();
                    string value = string.Join(':', pieces.Skip(1)).Trim();
                    switch (key)
                    {
                        case "output":
                            outputPath = value;
                            break;

                        case "language":
                            language = value;
                            break;

                        default:
                            throw new Exception("Unknown command line flag: --" + key);
                    }
                }
                else if (entryFile == null)
                {
                    entryFile = arg;
                }
                else
                {
                    throw new BuildException("Unknown command line argument: " + arg);
                }
            }

            output["source"] = entryFile ?? throw new BuildException("No code entrypoint file specified.");
            output["language"] = language ?? throw new BuildException("No output language specified. Use --language:{LANG}");
            output["destination"] = outputPath ?? throw new BuildException("No output directory specified. Use --output:{PATH}");

            return output;
        }

        private static string[] GetEffectiveArgs()
        {
#if DEBUG
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string binDebug = System.IO.Path.Combine("src", "bin", "Debug");
            if (currentDirectory.Contains(binDebug))
            {
                string srcRoot = currentDirectory.Split(binDebug)[0];
                string debugArgsFile = System.IO.Path.Combine(srcRoot, "DEBUG_ARGS.txt");
                if (System.IO.File.Exists(debugArgsFile))
                {
                    string text = System.IO.File.ReadAllText(debugArgsFile);
                    string[] lines = text.Trim().Split('\n');
                    string lastLine = lines[lines.Length - 1].Trim();
                    if (lastLine.Length > 0)
                    {
                        string[] debugArgs = lastLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (debugArgs.Length > 0)
                        {
                            return debugArgs;
                        }
                    }
                }
            }
#endif
            return null;
        }
    }
}
