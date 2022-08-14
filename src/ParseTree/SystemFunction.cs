namespace Vanilla.ParseTree
{
    public enum SystemFunctionType
    {
        UNKNOWN,

        ARRAY_CAST_FROM,
        ARRAY_LENGTH,
        ARRAY_OF,
        FLOOR,
        LIST_ADD,
        LIST_LENGTH,
        LIST_OF,
        LIST_TO_ARRAY,
        MAP_OF,
        SQRT,
        STRING_REPLACE,
        STRING_TO_CHARACTER_ARRAY,
        STRING_TRIM,
    }

    internal class SystemFunction : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public SystemFunctionType SystemId { get; private set; }
        public Type FunctionReturnType { get; private set; }
        public Expression RootContext { get; set; }

        public SystemFunction(Token firstToken, SystemFunctionType typeMethod, Type baseType, Token nameToken) : base(firstToken)
        {
            this.NameToken = nameToken;
            this.Name = baseType.RootType + "." + nameToken.Value;
            this.SystemId = typeMethod;
            this.FunctionReturnType = baseType;
        }

        public SystemFunction(Token dollarToken, Token nameToken) : base(dollarToken)
        {
            this.RootContext = null;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.SystemId = IdentifyFunction(dollarToken, this.Name);
            this.FunctionReturnType = GetFunctionReturnType(this.SystemId);
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

        private static Type GetFunctionReturnType(SystemFunctionType name)
        {
            switch (name)
            {
                case SystemFunctionType.FLOOR: return Type.INT;
                case SystemFunctionType.SQRT: return Type.FLOAT;
                default: throw new System.Exception();
            }
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            return this;
        }
    }
}
