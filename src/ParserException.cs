namespace Vanilla
{
    internal class ParserException : BuildException
    {
        public ParserException(string filename, string message) : base(filename + ": " + message) { }
        public ParserException(Token token, string message) : base(ConstructMessagePrefixFromToken(token) + message) { }
        public ParserException(ParseTree.Entity entity, string message) : base(ConstructMessagePrefixFromToken(entity.FirstToken) + message) { }

        private static string ConstructMessagePrefixFromToken(Token token)
        {
            return token.FileName + ", Line " + token.Line + ", Col " + token.Col + ": ";
        }

        internal static void EnsureArgCount(ParseTree.Entity entity, int expectedCount, int actualCount)
        {
            EnsureArgCount(entity.FirstToken, expectedCount, actualCount);
        }

        internal static void EnsureArgCount(Token token, int expectedCount, int actualCount)
        {
            if (expectedCount != actualCount)
            {
                throw new ParserException(token, "Incorrect number of arguments. Expected " + expectedCount + " but found " + actualCount + " instead.");
            }
        }
    }
}
