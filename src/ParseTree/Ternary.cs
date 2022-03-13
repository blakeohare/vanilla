namespace Vanilla.ParseTree
{
    internal class Ternary : Expression
    {
        public Expression Condition { get; private set; }
        public Expression TrueValue { get; private set; }
        public Expression FalseValue { get; private set; }

        public Ternary(Expression condition, Expression trueValue, Expression falseValue) : base(condition.FirstToken)
        {
            this.Condition = condition;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Condition = this.Condition.ResolveVariables(resolver, scope);
            this.TrueValue = this.TrueValue.ResolveVariables(resolver, scope);
            this.FalseValue = this.FalseValue.ResolveVariables(resolver, scope);
            return this;
        }
    }
}
