namespace Vanilla.ParseTree
{
    public enum SystemFunctionType
    {
        ARRAY_CAST_FROM,
        ARRAY_OF,
        FLOOR,
        LIST_ADD,
        LIST_OF,
        LIST_TO_ARRAY,
        MAP_OF,
        SQRT,
    }

    internal class SystemFunction : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public SystemFunctionType SystemId { get; private set; }
        public Type TypeForMethods { get; private set; }

        public SystemFunction(Token firstToken, SystemFunctionType typeMethod, Type baseType, Token nameToken) : base(firstToken)
        {
            this.NameToken = nameToken;
            this.Name = baseType.RootType + "." + nameToken.Value;
            this.SystemId = typeMethod;
            this.TypeForMethods = baseType;
        }

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

        public override Expression ResolveTypes(Resolver resolver)
        {
            throw new ParserException(this.FirstToken, "Type methods and system functions must be invoked and cannot be passed as function pointers.");
        }
    }
}
