namespace Vanilla.ParseTree
{
    internal class IntegerConstant : Expression
    {
        public int Value { get; private set; }

        public IntegerConstant(Token token, int value) : base(token)
        {
            this.Value = value;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.ResolvedType = Type.INT;
        }
    }
}
