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
            this.Root = this.Root.ResolveVariables(resolver, scope);
            for (int i = 0; i < this.ArgList.Length; i++)
            {
                this.ArgList[i] = this.ArgList[i].ResolveVariables(resolver, scope);
            }
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.Root.ResolveTypes(resolver);
            Type funcType = this.Root.ResolvedType;
            if (funcType.RootType != "func") throw new ParserException(this.Root.FirstToken, "This expression cannot be invoked like a function.");
            int expectedArgCount = funcType.Generics.Length - 1;
            int actualArgCount = this.ArgList.Length;
            if (expectedArgCount != actualArgCount) throw new ParserException(this.OpenParen, "This function has the wrong number of arguments. Expected " + expectedArgCount + " but found " + actualArgCount + ".");

            for (int i = 0; i < this.ArgList.Length; i++)
            {
                this.ArgList[i].ResolveTypes(resolver);
                Type actualType = this.ArgList[i].ResolvedType;
                Type expectedType = funcType.Generics[i + 1];
                if (!expectedType.AssignableFrom(actualType))
                {
                    throw new ParserException(
                        this.ArgList[i].FirstToken,
                        "Incorrect argument type. Expected '" + expectedType.ToString() + "' but received '" + actualType.ToString() + "' in argument " + (i + 1));
                }

            }

            this.ResolvedType = funcType.Generics[0];
        }
    }
}
