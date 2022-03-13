namespace Vanilla
{
    internal class TypeParser
    {
        private TokenStream tokens;

        public TypeParser(TokenStream tokens)
        {
            this.tokens = tokens;
        }

        public Type ParseType()
        {
            this.tokens.EnsureNotEof();
            Type t = this.TryParseType();
            if (t == null)
            {
                throw new ParserException(this.tokens.Peek(), "Expected a type");
            }
            return t;
        }

        public Type TryParseType()
        {
            int state = this.tokens.SnapshotState();
            this.tokens.EnableTypeParsingMode();
            Type type = this.TryParseTypeImpl();
            if (type != null) return type;
            this.tokens.RestoreState(state);
            this.tokens.EnableNormalParsingMode();
            return null;
        }

        private Type TryParseTypeImpl()
        {
            if (!tokens.HasMore) return null;
            Token firstToken = tokens.Pop();
            string next = firstToken.Value;
            switch (next)
            {
                case "int":
                case "bool":
                case "void":
                case "string":
                case "float":
                case "object":
                    return new Type() { FirstToken = firstToken, RootType = next, };

                case "array":
                case "list":
                case "set":
                    if (!tokens.PopIfPresent("<")) return null;
                    Type listType = this.TryParseTypeImpl();
                    if (listType == null) return null;
                    if (!tokens.PopIfPresent(">")) return null;
                    return new Type() { FirstToken = firstToken, RootType = next, Generics = new Type[] { listType } };

                case "map":
                    if (!tokens.PopIfPresent("<")) return null;
                    Type keyType = this.TryParseTypeImpl();
                    if (keyType == null) return null;
                    if (!tokens.PopIfPresent(",")) return null;
                    Type valueType = this.TryParseTypeImpl();
                    if (valueType == null) return null;
                    if (!tokens.PopIfPresent(">")) return null;
                    return new Type() { FirstToken = firstToken, RootType = next, Generics = new Type[] { keyType, valueType } };

                default:
                    // Yup, class names MUST begin with a capital letter
                    if (firstToken.Type != TokenType.WORD) return null;
                    if (next[0] < 'A' || next[0] > 'Z') return null;
                    return new Type() { FirstToken = firstToken, RootType = next, IsClass = true };
            }
        }
    }
}
