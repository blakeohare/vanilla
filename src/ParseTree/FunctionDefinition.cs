using System.Collections.Generic;

namespace Vanilla.ParseTree
{
    internal class FunctionDefinition : TopLevelEntity
    {
        public Token FunctionToken { get; private set; }
        public bool IsPublic { get; private set; }
        public Type ReturnType { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public VariableDeclaration[] Args { get; private set; }
        public Executable[] Body { get; set; }

        public Type SignatureType { get; private set; }

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
            int argCount = argNames.Count;
            this.Args = new VariableDeclaration[argCount];
            for (int i = 0; i < argCount; i++)
            {
                this.Args[i] = new VariableDeclaration(this, argDeclarations[i], argTypes[i], argNames[i], null, null);
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

        public void ResolveSignatureTypes(Resolver resolver)
        {
            this.ReturnType.Resolve(resolver);
            List<Type> signatureGenerics = new List<Type>() { this.ReturnType };
            foreach (VariableDeclaration arg in this.Args)
            {
                arg.Type.Resolve(resolver);
                signatureGenerics.Add(arg.Type);
            }

            this.SignatureType = new Type() { FirstToken = this.FirstToken, Generics = signatureGenerics.ToArray(), RootType = "func", IsResolved = true };
        }

        public void ResolveTypes(Resolver resolver)
        {
            foreach (Executable line in this.Body)
            {
                line.ResolveTypes(resolver);
            }
        }
    }
}