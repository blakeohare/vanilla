namespace Vanilla.ParseTree
{
    internal class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(TopLevelEntity owner, Expression expr) : base(expr.FirstToken, owner)
        {
            this.Expression = expr;
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Expression = this.Expression.ResolveVariables(resolver, scope);
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.Expression = this.Expression.ResolveTypes(resolver, null);
        }
    }
}
