using System;

namespace Vanilla
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO: arg checking and DEBUG_ARGS file support.
            string sourceDir = args[0];

            new Parser().Parse(sourceDir);
        }
    }
}
