namespace Vanilla.ParseTree
{
    internal class BooleanNot : Expression
    {
        public Expression Root { get; private set; }

        public BooleanNot(Token notToken, Expression root) : base(notToken)
        {
            this.Root = root;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.Root = this.Root.ResolveTypes(resolver);
            if (this.Root.ResolvedType.RootType != "bool") throw new ParserException(this, "! cannot be applied to this type.");
            return this;
        }
    }
}
