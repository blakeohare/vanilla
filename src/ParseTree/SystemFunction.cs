namespace Vanilla.ParseTree
{
    public enum SystemFunctionType
    {
        FLOOR,
        SQRT,
    }

    internal class SystemFunction : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public SystemFunctionType SystemId { get; private set; }

        public SystemFunction(Token dollarToken, Token nameToken) : base(dollarToken)
        {
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.SystemId = IdentifyFunction(dollarToken, this.Name);
        }

        private static SystemFunctionType IdentifyFunction(Token dollarToken, string name)
        {
            switch (name)
            {
                case "floor": return SystemFunctionType.FLOOR;
                case "sqrt": return SystemFunctionType.SQRT;
                default: throw new ParserException(dollarToken, "There is no system function by the name of '" + name + "'.");
            }
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }
    }
}
