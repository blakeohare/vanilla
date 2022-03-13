namespace Vanilla.ParseTree
{
    internal class NullConstant : Expression
    {
        public NullConstant(Token nullToken) : base(nullToken) { }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
