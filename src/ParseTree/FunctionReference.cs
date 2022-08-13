namespace Vanilla.ParseTree
{
    internal class FunctionReference : Expression
    {
        public FunctionDefinition FunctionDefinition { get; private set; }
        public Expression InstanceContext { get; set; }

        public FunctionReference(Token firstToken, FunctionDefinition funcDef, Type signatureType) : base(firstToken)
        {
            this.FunctionDefinition = funcDef;
            this.ResolvedType = signatureType;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.Exception();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            if (this.InstanceContext != null)
            {
                // Generated in variable resolver phase for local function references,
                // but generated in the type resolver phase for instance methods. Throw
                // only if this is an intance method that is making its way to the type
                // resolver phase.
                throw new System.Exception();
            }
            return this;
        }
    }
}
