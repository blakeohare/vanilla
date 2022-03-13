namespace Vanilla
{
    internal enum TokenType
    {
        WORD,
        STRING,
        PUNC,
        KEYWORD,
        NUMBER,
    }

    internal class Token
    {
        public string Value { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
        public int InternalCount { get; set; }
        public TokenType Type { get; set; }

        public Token()
        {
            this.InternalCount = 1;
        }

        public override string ToString()
        {
            return "Token: '" + this.Value + "'";
        }

        public bool IsImmediatelyAfter(Token previousToken)
        {
            if (previousToken.Col + previousToken.Value.Length == this.Col)
            {
                if (previousToken.Line == this.Line && previousToken.FileName == this.FileName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
