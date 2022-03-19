using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ConstructorInvocation : Expression
    {
        public Token ClassNameToken { get; private set; }
        public Expression[] Args { get; private set; }

        public ConstructorInvocation(Token newToken, Token classNameToken, IList<Expression> args) : base(newToken)
        {
            this.ClassNameToken = classNameToken;
            this.Args = args.ToArray();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
