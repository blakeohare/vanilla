using System;

namespace Vanilla
{
    internal class ParserException : Exception
    {
        public ParserException(string filename, string message) : base(filename + ": " + message) { }
        public ParserException(Token token, string message) : base(ConstructMessagePrefixFromToken(token) + message) { }
        public ParserException(ParseTree.Entity entity, string message) : base(ConstructMessagePrefixFromToken(entity.FirstToken) + message) { }

        private static string ConstructMessagePrefixFromToken(Token token)
        {
            return token.FileName + ", Line " + token.Line + ", Col " + token.Col + ": ";
        }
    }
}
