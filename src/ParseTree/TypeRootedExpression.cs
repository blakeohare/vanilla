namespace Vanilla.ParseTree
{
    internal class TypeRootedExpression : Expression
    {
        public Type Type { get; private set; }

        public TypeRootedExpression(Type type) : base(type.FirstToken)
        {
            this.Type = type;
        }
    }
}
