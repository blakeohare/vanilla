namespace Vanilla.ParseTree
{
    internal class ReturnStatement : Executable
    {
        public Expression Value { get; private set; }

        public ReturnStatement(Token returnToken, Expression optionalValue, TopLevelEntity owner) : base(returnToken, owner)
        {
            this.Value = optionalValue;
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            if (this.Value != null)
            {
                this.Value = this.Value.ResolveVariables(resolver, scope);
            }
        }
    }
}
