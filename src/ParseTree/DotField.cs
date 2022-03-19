namespace Vanilla.ParseTree
{
    internal class DotField : Expression
    {
        public Expression Root { get; set; }
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

        private SystemFunction CreateTypeMethod(SystemFunctionType func)
        {
            return new SystemFunction(this.Root.FirstToken, func, ((TypeRootedExpression)this.Root).Type, this.FieldToken);
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.Root = this.Root.ResolveTypes(resolver);

            if (this.Root is TypeRootedExpression)
            {
                Type rootType = ((TypeRootedExpression)this.Root).Type;
                switch (rootType.RootType + "." + this.FieldName)
                {
                    case "array.of": return CreateTypeMethod(SystemFunctionType.ARRAY_OF);
                    case "array.castFrom": return CreateTypeMethod(SystemFunctionType.ARRAY_CAST_FROM);
                    case "list.of": return CreateTypeMethod(SystemFunctionType.LIST_OF);
                    case "map.of": return CreateTypeMethod(SystemFunctionType.MAP_OF);

                    default:
                        throw new ParserException(this.DotToken, "The type '" + rootType + "' does not have a field named '" + this.FieldName + "'.");
                }
            }
            else
            {
                Type rootType = this.Root.ResolvedType;
                this.ResolvedType = this.GetPrimitiveFieldType(rootType, rootType.RootType + "." + this.FieldName);
                this.ResolvedType.Resolve(resolver);
            }
            return this;
        }

        private Type GetPrimitiveFieldType(Type rootType, string id)
        {
            Type itemType = rootType.Generics.Length == 1 ? rootType.Generics[0] : null;
            switch (id)
            {
                case "list.add": return Type.GetFunctionType(Type.VOID, new Type[] { itemType });
                case "list.toArray": return Type.GetFunctionType(Type.GetArrayType(itemType), new Type[0]);

                default:
                    throw new ParserException(this.DotToken, "There is no field named " + id + " on type " + rootType.RootType + ".");
            }
        }
    }
}
