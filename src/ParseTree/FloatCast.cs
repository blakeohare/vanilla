namespace Vanilla.ParseTree
{
    internal class FloatCast : Expression
    {
        public Expression Expression { get; private set; }

        public FloatCast(Token firstToken, Expression expr) : base(firstToken)
        {
            this.Expression = expr;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Expression = this.Expression.ResolveVariables(resolver, scope);
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.Expression = this.Expression.ResolveTypes(resolver, Type.INT);
            if (this.Expression.ResolvedType.IsFloat) return this.Expression;
            if (!this.Expression.ResolvedType.IsInteger) throw new ParserException(this, "Expected an integer here.");
            this.ResolvedType = Type.FLOAT;
            return this;
        }
    }
}
