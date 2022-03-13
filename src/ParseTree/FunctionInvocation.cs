using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class FunctionInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Expression[] ArgList { get; private set; }
        public Token OpenParen { get; private set; }

        public FunctionInvocation(Expression root, Token openParen, IList<Expression> argList) : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenParen = openParen;
            this.ArgList = argList.ToArray();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root.ResolveVariables(resolver, scope);
            foreach (Expression arg in this.ArgList)
            {
                arg.ResolveVariables(resolver, scope);
            }
            return this;
        }
    }
}
