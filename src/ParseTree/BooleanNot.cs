namespace Vanilla.ParseTree
{
    internal class BooleanNot : Expression
    {
        public Expression Root { get; private set; }

        public BooleanNot(Token notToken, Expression root) : base(notToken)
        {
            this.Root = root;
        }
    }
}
