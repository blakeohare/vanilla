namespace Vanilla.ParseTree
{
    internal class FloatConstant : Expression
    {
        public double Value { get; private set; }

        public FloatConstant(Token token, double value) : base(token)
        {
            this.Value = value;
            this.ResolvedType = Type.FLOAT;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            return this;
        }
    }
}
