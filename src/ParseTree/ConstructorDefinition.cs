using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ConstructorDefinition : TopLevelEntity
    {
        public Token[] ArgDeclarations { get; private set; }
        public Type[] ArgTypes { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Token BaseToken { get; private set; }
        public Expression[] BaseArgs { get; set; }
        public Executable[] Body { get; set; }

        public ConstructorDefinition(Token constructorToken, IList<Token> argDeclarations, IList<Type> argTypes, IList<Token> argNames, Token baseToken)
            : base(constructorToken)
        {
            this.ArgDeclarations = argDeclarations.ToArray();
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.BaseToken = baseToken;
            this.BaseArgs = new Expression[0];
        }

        public void ResolveArgTypes(Resolver resolver)
        {
            for (int i = 0; i < this.ArgTypes.Length; i++)
            {
                this.ArgTypes[i].Resolve(resolver);
            }
        }

        public void ResolveVariables(Resolver resolver)
        {
            LexicalScope rootScope = new LexicalScope(null);
            throw new System.NotImplementedException();
            /*
            // TODO: the args need to be variable declarations, like FunctionDefinition
            foreach (VariableDeclaration arg in this.ArgDeclarations)
            {
                rootScope.AddDefinition(arg);
            }*/

            // The arguments are directly in the root scope rather than creating a new one and setting its parent to it.
            // This intentionally will cause redeclaration of the args to induce a compile error.

            foreach (Executable line in this.Body)
            {
                line.ResolveVariables(resolver, rootScope);
            }
        }

        public void ResolveTypes(Resolver resolver)
        {
            for (int i = 0; i < this.BaseArgs.Length; i++)
            {
                this.BaseArgs[i] = this.BaseArgs[i].ResolveTypes(resolver);
            }

            foreach (Executable line in this.Body)
            {
                line.ResolveTypes(resolver);
            }
        }
    }
}
