using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class StringConcatChain : Expression
    {
        public Expression[] Expressions { get; private set; }

        public StringConcatChain(Token firstToken, IList<Expression> expressions) : base(firstToken)
        {
            this.Expressions = expressions.ToArray();
            this.ResolvedType = Type.STRING;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            // Generated during the type resolver phase
            throw new System.NotImplementedException();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            // Generated during the type resolver phase
            throw new System.NotImplementedException();
        }
    }
}
