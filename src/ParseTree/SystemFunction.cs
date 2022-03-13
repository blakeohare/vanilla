namespace Vanilla.ParseTree
{
    internal class SystemFunction : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }

        public SystemFunction(Token dollarToken, Token nameToken) : base(dollarToken)
        {
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
        }
    }
}
