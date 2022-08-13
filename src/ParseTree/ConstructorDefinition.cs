using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ConstructorDefinition : TopLevelEntity
    {
        public VariableDeclaration[] Args { get; private set; }
        public Token BaseToken { get; private set; }
        public Expression[] BaseArgs { get; set; }
        public Executable[] Body { get; set; }

        public ConstructorDefinition(Token constructorToken, IList<Token> argDeclarations, IList<Type> argTypes, IList<Token> argNames, Token baseToken)
            : base(constructorToken)
        {
            int argc = argNames.Count;
            List<VariableDeclaration> args = new List<VariableDeclaration>();
            for (int i = 0; i < argc; i++)
            {
                VariableDeclaration argDec = new VariableDeclaration(this, argDeclarations[i], argTypes[i], argNames[i], null, null);
                args.Add(argDec);
            }
            this.Args = args.ToArray();
            this.BaseToken = baseToken;
            this.BaseArgs = new Expression[0];
        }

        public void ResolveArgTypes(Resolver resolver)
        {
            for (int i = 0; i < this.Args.Length; i++)
            {
                this.Args[i].Type.Resolve(resolver);
            }
        }

        public void ResolveVariables(Resolver resolver)
        {
            LexicalScope rootScope = new LexicalScope(null);

            // The arguments are directly in the root scope rather than creating a new one and setting its parent to it.
            // This intentionally will cause redeclaration of the args to induce a compile error.
            foreach (VariableDeclaration arg in this.Args)
            {
                rootScope.AddDefinition(arg);
            }
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
