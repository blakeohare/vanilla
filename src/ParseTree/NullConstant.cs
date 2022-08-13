namespace Vanilla.ParseTree
{
    internal class NullConstant : Expression
    {
        public NullConstant(Token nullToken) : base(nullToken) { }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            if (nullHint == null || nullHint.IsBoolean || nullHint.IsNumeric) throw new ParserException(this, "Null cannot be used here.");
            this.ResolvedType = nullHint;
            return this;
        }
    }
}
