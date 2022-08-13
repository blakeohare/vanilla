using System.Linq;

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

            FunctionDefinition funcDef = resolver.GetFunctionByName(this.Name);
            if (funcDef != null)
            {
                FunctionReference fr = new FunctionReference(
                    this.NameToken,
                    funcDef,
                    Type.GetFunctionType(funcDef.ReturnType, funcDef.Args.Select(arg => arg.Type).ToArray()));
                
                return fr;
            }

            throw new ParserException(this, "The variable '" + this.Name + "' is not declared.");
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.ResolvedType = this.Declaration.Type;
            return this;
        }
    }
}
