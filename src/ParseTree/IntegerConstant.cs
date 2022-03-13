namespace Vanilla.ParseTree
{
    internal class IntegerConstant : Expression
    {
        public int Value { get; private set; }

        public IntegerConstant(Token token, int value) : base(token)
        {
            this.Value = value;
        }
    }
}
