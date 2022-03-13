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
    }
}
