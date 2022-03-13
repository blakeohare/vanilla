namespace Vanilla.ParseTree
{
    internal class NullableCast : Expression
    {
        public Expression Root { get; private set; }
        public Token AsToken { get; private set; }
        public Type TargetType { get; private set; }

        public NullableCast(Expression rootExpr, Token asToken, Type targetType) : base(rootExpr.FirstToken)
        {
            this.Root = rootExpr;
            this.AsToken = asToken;
            this.TargetType = targetType;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }
    }
}
