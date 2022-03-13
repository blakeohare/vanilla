using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class FunctionDefinition : TopLevelEntity
    {
        public Token FunctionToken { get; private set; }
        public bool IsPublic { get; private set; }
        public Type ReturnType { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public Token[] ArgDeclarations { get; private set; }
        public Type[] ArgTypes { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Executable[] Body { get; set; }

        public FunctionDefinition(
            Modifiers modifiers, 
            Token functionToken, 
            Type returnType, 
            Token nameToken, 
            IList<Token> argDeclarations, 
            IList<Type> argTypes, 
            IList<Token> argNames)
            : base(modifiers == null ? functionToken : modifiers.FirstToken)
        {
            this.FunctionToken = functionToken;
            this.IsPublic = modifiers != null && modifiers.IsPublic;
            this.ReturnType = returnType;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.ArgDeclarations = argDeclarations.ToArray();
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
        }

        public void ResolveVariables(Resolver resolver)
        {
            LexicalScope rootScope = new LexicalScope(null);
            for (int i = 0; i < this.ArgTypes.Length; i++)
            {
                VariableDeclaration argDecl = new VariableDeclaration(this, this.ArgDeclarations[i], this.ArgTypes[i], this.ArgNames[i], null, null);
                rootScope.AddDefinition(argDecl);
            }

            // The arguments are directly in the root scope rather than creating a new one and setting its parent to it.
            // This intentionally will cause redeclaration of the args to induce a compile error.

            foreach (Executable line in this.Body)
            {
                line.ResolveVariables(resolver, rootScope);
            }
        }
    }
}
