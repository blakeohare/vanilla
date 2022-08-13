using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class MethodInvocation : Expression
    {
        public Expression[] ArgList { get; private set; }
        public FunctionReference FuncRef { get; private set; }
        public Expression InstanceContext { get; private set; }

        public MethodInvocation(Expression instance, FunctionReference funcRef, IList<Expression> args) : base(instance.FirstToken)
        {
            this.InstanceContext = instance;
            this.FuncRef = funcRef;
            this.ArgList = args.ToArray();
            this.ResolvedType = this.FuncRef.ResolvedType.Generics[0]; // The function signature's return type is the overall type of the invocation.
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            throw new NotImplementedException();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new NotImplementedException();
        }
    }
}
