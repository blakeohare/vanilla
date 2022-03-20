using System.Collections.Generic;
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

        public override Expression ResolveTypes(Resolver resolver)
        {
            for (int i = 0; i < this.Expressions.Length; i++)
            {
                this.Expressions[i] = this.Expressions[i].ResolveTypes(resolver);
            }

            Type accType = this.Expressions[0].ResolvedType;
            for (int i = 0; i < this.Ops.Length; i++)
            {
                accType = this.CombineTypesWithOp(accType, this.Ops[i], this.Expressions[i + 1].ResolvedType);
            }

            this.ResolvedType = accType;

            return this.GenerateMoreSpecificParseNode();
        }

        private enum OpGroup
        {
            PLUS,
            MULTIPLY,
            EQUALITY,
            INEQUALITY,
            BIT_SHIFT,
            BIT_MASK,
            BOOL_COMB,
        }

        private static Dictionary<string, OpGroup> opGroupsByToken = new Dictionary<string, OpGroup>() {
            { "+", OpGroup.PLUS },
            { "-", OpGroup.PLUS },
            { "*", OpGroup.MULTIPLY },
            { "/", OpGroup.MULTIPLY },
            { "%", OpGroup.MULTIPLY },
            { "==", OpGroup.EQUALITY },
            { "!=", OpGroup.EQUALITY },
            { "<", OpGroup.INEQUALITY },
            { ">", OpGroup.INEQUALITY },
            { "<=", OpGroup.INEQUALITY },
            { ">=", OpGroup.INEQUALITY },
            { "<<", OpGroup.BIT_SHIFT },
            { ">>", OpGroup.BIT_SHIFT },
            { "&", OpGroup.BIT_MASK },
            { "|", OpGroup.BIT_MASK },
            { "^", OpGroup.BIT_MASK },
            { "&&", OpGroup.BOOL_COMB },
            { "||", OpGroup.BOOL_COMB },
        };

        private Expression GenerateMoreSpecificParseNode()
        {
            OpGroup group = opGroupsByToken[this.Ops[0].Value];
            Type[] types = this.Expressions.Select(e => e.ResolvedType).ToArray();

            if (group == OpGroup.PLUS)
            {
                int firstString = -1;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].IsString)
                    {
                        firstString = i;
                        break;
                    }
                }

                if (firstString != -1)
                {
                    if (firstString < 2)
                    {
                        return new StringConcatChain(this.FirstToken, this.Expressions);
                    }
                    else
                    {
                        List<Expression> preMathExpr = new List<Expression>() { this.Expressions[0] };
                        List<Token> mathOps = new List<Token>();
                        for (int i = 1; i < firstString; i++)
                        {
                            preMathExpr.Add(this.Expressions[i]);
                            mathOps.Add(this.Ops[i - 1]);
                        }
                        OpChain subOpChain = new OpChain(preMathExpr, mathOps);
                        Expression[] strPieces = new List<Expression>() { subOpChain }.Concat(this.Expressions.Skip(firstString)).ToArray();

                        return new StringConcatChain(this.FirstToken, strPieces);
                    }
                }
            }

            if (this.Expressions.Length > 2 && (group == OpGroup.INEQUALITY || group == OpGroup.EQUALITY))
            {
                throw new ParserException(this, "Cannot use comparisons in a series of more than two expressions.");
            }

            if (group == OpGroup.MULTIPLY)
            {
                Expression innermost = new ArithmeticPairOp(this.Expressions[0], this.Ops[0], this.Expressions[1]);
                
                for (int i = 2; i < this.Expressions.Length; i++)
                {
                    innermost = new ArithmeticPairOp(innermost, this.Ops[i - 1], this.Expressions[i]);
                }
                return innermost;
            }

            return this; // I think everything else is fine...I think
        }

        private Type CombineTypesWithOp(Type leftType, Token opToken, Type rightType)
        {
            string op = opToken.Value;
            string left = leftType.RootType;
            string right = rightType.RootType;

            switch (op)
            {
                case "+":
                    if (left == "string" || right == "string")
                    {
                        return Type.STRING;
                    }
                    break;

                case "==":
                case "!=":
                    return Type.BOOL;

                case "&&":
                case "||":
                    if (left == "bool" && right == "bool") return Type.BOOL;
                    throw new ParserException(opToken, "The " + op + " operator is not supported for non-boolean types.");
            }

            bool leftIsNum = left == "int" || left == "float";
            bool rightIsNum = right == "int" || right == "float";

            if (leftIsNum && rightIsNum)
            {
                bool intMath = left == "int" && right == "int";
                Type combinedMathType = intMath ? Type.INT : Type.FLOAT;
                switch (op)
                {
                    case "+":
                    case "-":
                    case "/":
                    case "*":
                    case "%":
                        return combinedMathType;

                    case "&":
                    case "|":
                    case "^":
                    case ">>":
                    case "<<":
                        if (intMath) return Type.INT;
                        throw new ParserException(opToken, "The " + op + " operator is not supported for non-integer types. Use $floor if necessary.");

                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        return Type.BOOL;
                }
            }

            throw new ParserException(opToken, "The " + op + " operator is not supported for types " + left + " and " + right + ".");
        }
    }
}
