namespace Vanilla.ParseTree
{
    internal class BooleanConstant : Expression
    {
        public bool Value { get; private set; }

        public BooleanConstant(Token token, bool value) : base(token)
        {
            this.Value = value;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.ResolvedType = Type.BOOL;
            return this;
        }
    }
}
