namespace Vanilla.ParseTree
{
    internal class IntegerConstant : Expression
    {
        public int Value { get; private set; }

        public IntegerConstant(Token token, int value) : base(token)
        {
            this.Value = value;
            this.ResolvedType = Type.INT;
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
