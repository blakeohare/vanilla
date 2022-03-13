namespace Vanilla.ParseTree
{
    internal class DotField : Expression
    {
        public Expression Root { get; private set; }
        public Token DotToken { get; private set; }
        public Token FieldToken { get; private set; }
        public string FieldName { get; private set; }

        public DotField(Expression root, Token dotToken, Token fieldToken) : base(root.FirstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldToken = fieldToken;
            this.FieldName = fieldToken.Value;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            return this;
        }
    }
}
