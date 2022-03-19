namespace Vanilla.ParseTree
{
    internal class BracketIndex : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenBracket { get; private set; }
        public Expression Index { get; private set; }

        public BracketIndex(Expression root, Token openBracket, Expression index) : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenBracket = openBracket;
            this.Index = index;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            this.Index = this.Index.ResolveVariables(resolver, scope);
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.Root = this.Root.ResolveTypes(resolver);
            this.Index = this.Index.ResolveTypes(resolver);
            Type rootType = this.Root.ResolvedType;
            Type keyType = this.Index.ResolvedType;
            switch (this.Root.ResolvedType.RootType)
            {
                case "array":
                    if (keyType.RootType != "int") throw new ParserException(this.Index, "Cannot index into an array with this type. Index must be an integer.");
                    return new ArrayIndex(this.Root, this.OpenBracket, this.Index);

                case "map":
                    Type expectedKeyType = rootType.Generics[0];
                    if (!rootType.Generics[0].AssignableFrom(keyType)) throw new ParserException(this.Index, "Cannot key into a map with this type. Expected type is " + expectedKeyType);
                    this.ResolvedType = rootType.Generics[1];
                    break;
                case "string":
                    if (keyType.RootType != "int") throw new ParserException(this.Index, "Cannot index into a string with this type. Index must be an integer.");
                    this.ResolvedType = Type.STRING;
                    break;
                default:
                    throw new ParserException(this.OpenBracket, "Indexing is not available for this type.");
            }
            return this;
        }
    }
}
