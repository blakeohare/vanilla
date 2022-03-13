namespace Vanilla.ParseTree
{
    internal class NullConstant : Expression
    {
        public NullConstant(Token nullToken) : base(nullToken) { }
    }
}
