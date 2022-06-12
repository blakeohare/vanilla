using System;
using System.Collections.Generic;

namespace Vanilla
{
    internal static class FileUtil
    {
        public static void EnsureDirectoryExists(string path)
        {
            string absPath = NormalizeToAbsolute(path);
            EnsureDirExistsImpl(absPath);
        }

        private static void EnsureDirExistsImpl(string path)
        {
            if (System.IO.Directory.Exists(path)) return;
            EnsureDirExistsImpl(System.IO.Path.GetDirectoryName(path));
            System.IO.Directory.CreateDirectory(path);
        }

        public static string NormalizeToAbsolute(string path)
        {
            if (path.Length >= 2 && path[1] != ':')
            {
                path = System.IO.Directory.GetCurrentDirectory() + "/" + path;
            }

            path = path.Replace('\\', '/');
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }

            List<string> pathBuilder = new List<string>();
            foreach (string pathPart in path.Split("/"))
            {
                if (pathPart == "..")
                {
                    if (pathBuilder.Count <= 1) throw new InvalidOperationException("Invalid path");
                    pathBuilder.RemoveAt(pathBuilder.Count - 1);
                }
                else if (pathPart == ".")
                {
                    // do nothing
                }
                else
                {
                    pathBuilder.Add(pathPart);
                }
            }
            return string.Join(System.IO.Path.DirectorySeparatorChar, pathBuilder);
        }

        public static string FileReadText(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }
            return null;
        }
    }
}
