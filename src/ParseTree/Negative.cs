namespace Vanilla.ParseTree
{
    internal class Negative : Expression
    {
        public Expression Root { get; private set; }

        public Negative(Token minusToken, Expression root) : base(minusToken)
        {
            this.Root = root;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
