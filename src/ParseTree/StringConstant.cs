namespace Vanilla.ParseTree
{
    internal class StringConstant : Expression
    {
        public int StringTableEntryId { get; set; }
        public string Value { get; private set; }

        public StringConstant(Token token, string value) : base(token)
        {
            this.Value = value;
            this.ResolvedType = Type.STRING;
            this.StringTableEntryId = -1;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.StringTableEntryId = resolver.RegisterStringConstant(this.Value);
            return this;
        }
    }
}
