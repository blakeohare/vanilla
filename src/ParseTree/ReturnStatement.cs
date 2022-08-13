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

        public override void ResolveTypes(Resolver resolver)
        {
            if (this.Value != null)
            {
                Type nullHint;
                if (this.Owner is FunctionDefinition)
                {
                    nullHint = ((FunctionDefinition)this.Owner).ReturnType;
                }
                else if (this.Owner is ConstructorDefinition)
                {
                    throw new ParserException(this, "Cannot return a value from the constructor");
                }
                else
                {
                    throw new ParserException(this, "The return statement cannot be used here.");
                }

                this.Value = this.Value.ResolveTypes(resolver, nullHint);
            }
        }
    }
}
