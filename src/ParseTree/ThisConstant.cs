namespace Vanilla.ParseTree
{
    internal class ThisConstant : Expression
    {
        private TopLevelEntity owner;

        public ThisConstant(Token thisToken, TopLevelEntity owner)
            : base(thisToken)
        {
            this.owner = owner;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            if (this.owner == null || // should not happen but who knows.
                this.owner.WrapperClass == null)
            {
                throw new ParserException(this, "'this' keyword cannot be used here. It can only be used inside a class.");
            }

            Type currentClassType = Type.GetInstanceType(this.owner.WrapperClass.Name);
            currentClassType.Resolve(resolver);
            this.ResolvedType = currentClassType;
            return this;
        }
    }
}
