namespace Vanilla.ParseTree
{
    internal class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(TopLevelEntity owner, Expression expr) : base(expr.FirstToken, owner)
        {
            this.Expression = expr;
        }
    }
}
