namespace Vanilla.ParseTree
{
    internal class TypeRootedExpression : Expression
    {
        public Type Type { get; private set; }

        public TypeRootedExpression(Type type) : base(type.FirstToken)
        {
            this.Type = type;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.Type.Resolve(resolver);
            this.ResolvedType = Type.TYPE;
            return this;
        }
    }
}
