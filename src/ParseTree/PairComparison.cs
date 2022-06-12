using System;

namespace Vanilla.ParseTree
{
    internal class PairComparison : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public Token Op { get; set; }

        public PairComparison(Token firstToken, Expression left, Expression right, Token op) 
            :base(firstToken)
        {
            this.Left = left;
            this.Right = right;
            this.Op = op;
            this.ResolvedType = Type.BOOL;
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            throw new NotImplementedException(); // Created in the type resolver phase
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new NotImplementedException();
        }
    }
}
