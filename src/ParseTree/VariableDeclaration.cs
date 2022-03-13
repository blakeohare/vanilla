namespace Vanilla.ParseTree
{
    internal class VariableDeclaration : Executable
    {
        public bool IsConst { get; private set; }
        public Type Type { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public Token AssignOp { get; private set; }
        public Expression InitialValue { get; private set; }

        public VariableDeclaration(TopLevelEntity owner, Token declaration, Type type, Token name, Token assignOp, Expression initialValue) : base(declaration, owner)
        {
            this.IsConst = declaration.Value == "const";
            this.Type = type;
            this.NameToken = name;
            this.Name = name.Value;
            this.AssignOp = assignOp;
            this.InitialValue = initialValue;
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            if (this.InitialValue != null)
            {
                this.InitialValue.ResolveVariables(resolver, scope);
            }
            scope.AddDefinition(this);
        }
    }
}
