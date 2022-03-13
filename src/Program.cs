using System;
using Vanilla.Transpiler;

namespace Vanilla
{
    internal class Program
    {
        static void Main(string[] args)
        {
            args = GetEffectiveArgs() ?? args;

            string sourceEntryFile = args[0];

            ParseBundle bundle = new Parser().Parse(sourceEntryFile);

            // TODO: use args 
            string outputFile = new CTranspiler(bundle).GenerateFile();
            System.IO.File.WriteAllText("test.h", outputFile);
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
