namespace Vanilla.ParseTree
{
    internal class StringConstant : Expression
    {
        public string Value { get; private set; }

        public StringConstant(Token token, string value) : base(token)
        {
            this.Value = value;
            this.ResolvedType = Type.STRING;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            return this;
        }
    }
}
