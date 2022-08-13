namespace Vanilla.ParseTree
{
    internal class FieldReference : Expression
    {
        public Field FieldDefinition{ get; private set; }
        public Expression InstanceContext { get; set; }

        public FieldReference(Token firstToken, Field fieldDef, Expression instCtx) : base(firstToken)
        {
            this.FieldDefinition = fieldDef;
            this.ResolvedType = fieldDef.Type;
            this.InstanceContext = instCtx;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.Exception();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            throw new System.Exception(); // generated in the type resolver phase
        }
    }
}
