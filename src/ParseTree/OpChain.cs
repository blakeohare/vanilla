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
            return this;
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
