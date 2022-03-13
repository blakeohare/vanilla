namespace Vanilla.ParseTree
{
    internal class Assignment : Executable
    {
        public Expression Target { get; private set; }
        public Token Op { get; private set; }
        public Expression Value { get; private set; }

        public Assignment(Expression target, Token op, Expression value, TopLevelEntity owner) : base(target.FirstToken, owner)
        {
            this.Target = target;
            this.Op = op;
            this.Value = value;
        }
    }
}
