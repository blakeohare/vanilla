using System;

namespace Vanilla.ParseTree
{
    internal class ArrayIndex : Expression
    {
        public Expression Root { get; private set; }
        public Expression Index { get; private set; }
        public Token BracketToken { get; private set; }

        public ArrayIndex(Expression root, Token bracketToken, Expression index) : base(root.FirstToken)
        {
            this.Root = root;
            this.BracketToken = bracketToken;
            this.Index = index;
            this.ResolvedType = this.Root.ResolvedType.Generics[0];
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            // Generated as a result of type resolver, so children are already resolved.
            throw new NotImplementedException();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            // Created after this phase
            throw new NotImplementedException();
        }
    }
}
