namespace Vanilla.ParseTree
{
    internal class InlineIncrement : Expression
    {
        public Expression Root { get; private set; }
        public Token IncrementToken { get; private set; }
        public bool IsAddition { get { return this.IncrementToken.Value == "++"; } }
        public bool IsPrefix { get; private set; }

        public InlineIncrement(Expression root, Token incrementToken, bool isPrefix) : base(isPrefix ? incrementToken : root.FirstToken)
        {
            this.Root = root;
            this.IncrementToken = incrementToken;
            this.IsPrefix = isPrefix;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            throw new System.NotImplementedException();
        }
    }
}
