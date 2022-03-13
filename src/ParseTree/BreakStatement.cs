namespace Vanilla.ParseTree
{
    internal class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, TopLevelEntity owner) : base(breakToken, owner) { }
    }
}
