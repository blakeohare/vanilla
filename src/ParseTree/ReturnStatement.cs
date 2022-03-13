namespace Vanilla.ParseTree
{
    internal class ReturnStatement : Executable
    {
        public Expression Value { get; private set; }

        public ReturnStatement(Token returnToken, Expression optionalValue, TopLevelEntity owner) : base(returnToken, owner)
        {
            this.Value = optionalValue;
        }
    }
}
