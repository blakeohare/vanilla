namespace Vanilla.ParseTree
{
    internal class FloatConstant : Expression
    {
        public double Value { get; private set; }

        public FloatConstant(Token token, double value) : base(token)
        {
            this.Value = value;
        }
    }
}
