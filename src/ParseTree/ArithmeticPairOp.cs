using System;

namespace Vanilla.ParseTree
{
    // Extracted from an OpChain of various operations that are simpler to transpiler if they're
    // restricted to just situations with a clear left and right operator. Specifically ones where
    // it's good to know if the overeall type of the pair will be a float or int based on the
    // constituent pieces.
    internal class ArithmeticPairOp : Expression
    {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public Token Op { get; private set; }
        public bool IsModulo { get { return this.Op.Value == "%"; } }
        public bool IsNativeModuloOkay
        {
            get
            {
                return this.IsModulo &&
                    ((this.Right is IntegerConstant && ((IntegerConstant)this.Right).Value > 0) ||
                        this.Right is FloatConstant && ((FloatConstant)this.Right).Value > 0);
            }
        }

        public ArithmeticPairOp(Expression left, Token op, Expression right) : base(left.FirstToken)
        {
            this.Left = left;
            this.Right = right;
            this.Op = op;
            this.ResolvedType = left.ResolvedType.IsFloat || right.ResolvedType.IsFloat ? Type.FLOAT : Type.INT;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            // generated in the type resolver phase
            throw new NotImplementedException();
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            // generated in the type resolver phase
            throw new NotImplementedException();
        }
    }
}
