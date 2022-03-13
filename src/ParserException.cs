using System;

namespace Vanilla
{
    internal class ParserException : Exception
    {
        public ParserException(string filename, string message) : base(filename + ": " + message) { }
        public ParserException(Token token, string message) : base(ConstructMessagePrefixFromToken(token) + message) { }

        private static string ConstructMessagePrefixFromToken(Token token)
        {
            return token.FileName + ", Line " + token.Line + ", Col " + token.Col + ": ";
        }
    }
}
