using System;

namespace Vanilla.ParseTree
{
    internal class MapAccess : Expression
    {
        public Expression Root { get; private set; }
        public Expression Key { get; private set; }
        public Token OpenBracket { get; private set; }

        public MapAccess(Expression root, Token openBracket, Expression key) : base(root.FirstToken)
        {
            this.Root = root;
            this.Key = key;
            this.OpenBracket = openBracket;
            this.ResolvedType = this.Root.ResolvedType.Generics[1];
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            // This expression is created after this point
            throw new NotImplementedException();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            // Created during this phase so types are already resolved
            throw new NotImplementedException();
        }
    }
}
