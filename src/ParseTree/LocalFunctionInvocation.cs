using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class LocalFunctionInvocation : Expression
    {
        public Expression[] ArgList { get; private set; }
        public FunctionReference FuncRef { get; private set; }

        public LocalFunctionInvocation(FunctionReference funcRef, IList<Expression> args) : base(funcRef.FirstToken)
        {
            this.FuncRef = funcRef;
            this.ArgList = args.ToArray();
            this.ResolvedType = this.FuncRef.ResolvedType.Generics[0]; // The function signature's return type is the overall type of the invocation.
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            throw new NotImplementedException();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new NotImplementedException();
        }
    }
}
