namespace Vanilla.ParseTree
{
    internal class Variable : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public VariableDeclaration Declaration { get; private set; }

        public Variable(Token nameToken, string name) : base(nameToken)
        {
            this.NameToken = nameToken;
            this.Name = name;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            VariableDeclaration varDecl = scope.TryGetDeclaration(this.Name);
            if (varDecl != null)
            {
                this.Declaration = varDecl;
                return this;
            }

            FunctionDefinition fd = resolver.GetFunctionByName(this.Name);
            if (fd != null)
            {
                return new FunctionReference(this.NameToken, fd);
            }

            throw new ParserException(this, "The variable '" + this.Name + "' is not declared.");
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.ResolvedType = this.Declaration.Type;
            return this;
        }
    }
}
