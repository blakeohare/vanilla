namespace Vanilla.ParseTree
{
    internal class FunctionReference : Expression
    {
        public FunctionDefinition FunctionDefinition { get; private set; }

        public FunctionReference(Token firstToken, FunctionDefinition funcDef) : base(firstToken)
        {
            this.FunctionDefinition = funcDef;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.ResolvedType = this.FunctionDefinition.SignatureType;
            return this;
        }
    }
}
