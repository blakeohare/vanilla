namespace Vanilla.ParseTree
{
    internal class Negative : Expression
    {
        public Expression Root { get; private set; }

        public Negative(Token minusToken, Expression root) : base(minusToken)
        {
            this.Root = root;
        }
    }
}
