namespace Vanilla.ParseTree
{
    internal class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, TopLevelEntity owner) : base(breakToken, owner) { }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
