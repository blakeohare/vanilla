using System;
using System.Collections.Generic;
using System.Text;

namespace Vanilla.ParseTree
{
    internal class BooleanCombination : Expression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public Token Op { get; private set; }

        public BooleanCombination(Expression left, Token op, Expression right) : base(left.FirstToken)
        {
            this.Left = left;
            this.Op = op;
            this.Right = right;
            this.ResolvedType = Type.BOOL;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new NotImplementedException();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            throw new NotImplementedException();
        }
    }
}
