namespace Vanilla.ParseTree
{
    internal class BracketIndex : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenBracket { get; private set; }
        public Expression Index { get; private set; }

        public BracketIndex(Expression root, Token openBracket, Expression index) : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenBracket = openBracket;
            this.Index = index;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            this.Index = this.Index.ResolveVariables(resolver, scope);
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.Root.ResolveTypes(resolver);
            this.Index.ResolveTypes(resolver);
        }
    }
}
