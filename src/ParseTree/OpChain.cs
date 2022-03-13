﻿using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class OpChain : Expression
    {
        public Expression[] Expressions { get; private set; }
        public Token[] Ops { get; private set; }

        public OpChain(IList<Expression> expressions, IList<Token> ops) : base(expressions[0].FirstToken)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            for (int i = 0; i < this.Expressions.Length; i++)
            {
                this.Expressions[i] = this.Expressions[i].ResolveVariables(resolver, scope);
            }
            return this;
        }
    }
}
