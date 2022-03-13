namespace Vanilla.ParseTree
{
    internal class StringConstant : Expression
    {
        public string Value { get; private set; }

        public StringConstant(Token token, string value) : base(token)
        {
            this.Value = value;
        }
    }
}
