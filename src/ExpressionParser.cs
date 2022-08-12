using System;
using System.Collections.Generic;
using System.Text;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class ExpressionParser
    {
        private Parser parser;
        private TokenStream tokens;
        public EntityParser EntParser { get { return this.parser.EntParser; } }
        public ExecutableParser ExecParser { get { return this.parser.ExecParser; } }
        public ExpressionParser ExprParser { get { return this.parser.ExprParser; } }

        private OpChainParser boolComboParser;
        private OpChainParser bitwiseParser;
        private OpChainParser equalityParser;
        private OpChainParser inequalityParser;
        private OpChainParser bitShiftParser;
        private OpChainParser additionParser;
        private OpChainParser multiplicationParser;

        public ExpressionParser(Parser parser)
        {
            this.parser = parser;

            this.boolComboParser = new OpChainParser("&& ||".Split(' '));
            this.bitwiseParser = new OpChainParser("& | ^".Split(' '));
            this.equalityParser = new OpChainParser("== !=".Split(' '));
            this.inequalityParser = new OpChainParser("< > <= >=".Split(' '));
            this.bitShiftParser = new OpChainParser("<< >>".Split(' '));
            this.additionParser = new OpChainParser("+ -".Split(' '));
            this.multiplicationParser = new OpChainParser("* / %".Split(' '));

            this.boolComboParser.NextParser = this.bitwiseParser.Parse;
            this.bitwiseParser.NextParser = this.equalityParser.Parse;
            this.equalityParser.NextParser = this.inequalityParser.Parse;
            this.inequalityParser.NextParser = this.bitShiftParser.Parse;
            this.bitShiftParser.NextParser = this.additionParser.Parse;
            this.additionParser.NextParser = this.multiplicationParser.Parse;
            this.multiplicationParser.NextParser = this.ParseUnary;

        }

        public void SetTokens(TokenStream tokens)
        {
            this.tokens = tokens;
        }

        private TopLevelEntity activeOwner = null;
        public Expression ParseExpression(TopLevelEntity owner)
        {
            this.activeOwner = owner;
            return this.ParseTernary();
        }

        private Expression ParseTernary()
        {
            Expression root = this.boolComboParser.Parse(this.tokens);
            if (tokens.PopIfPresent("?"))
            {
                Expression trueExpr = this.ParseTernary();
                tokens.PopExpected(":");
                Expression falseExpr = this.ParseTernary();
                return new Ternary(root, trueExpr, falseExpr);
            }
            return root;
        }

        private Expression ParseUnary(TokenStream tokens)
        {
            if (tokens.IsNext("!"))
            {
                Token notToken = tokens.Pop();
                return new BooleanNot(notToken, this.ParseUnary(tokens));
            }

            if (tokens.IsNext("-"))
            {
                Token minusToken = tokens.Pop();
                return new Negative(minusToken, this.ParseUnary(tokens));
            }

            return this.ParseIncrement();
        }

        private Expression ParseIncrement()
        {
            Token prefix = null;
            Token suffix = null;
            string next = tokens.PeekValue();
            if (next == "++" || next == "--")
            {
                prefix = tokens.Pop();
            }

            Expression root = this.ParseParensAndSuffixes();

            next = tokens.PeekValue();
            if (prefix == null && (next == "++" || next == "--"))
            {
                suffix = tokens.Pop();
            }

            if (prefix != null)
            {
                return new InlineIncrement(root, prefix, true);
            }
            if (suffix != null)
            {
                return new InlineIncrement(root, suffix, false);
            }
            return root;
        }

        private Expression ParseParensAndSuffixes()
        {
            Expression root; 
            if (tokens.IsNext("("))
            {
                tokens.Pop();
                root = this.ParseExpression(this.activeOwner);
                tokens.PopExpected(")");
            } else
            {
                root = this.ParseAtomic();
            }

            bool parseSuffixes = true;
            while (parseSuffixes)
            {
                switch (tokens.PeekValue() ?? "")
                {
                    case ".":
                        Token dotToken = tokens.Pop();
                        Token fieldToken = tokens.PopNonNull();
                        if (fieldToken.Type != TokenType.WORD) throw new ParserException(fieldToken, "Expected field name");
                        root = new DotField(root, dotToken, fieldToken);
                        break;
                    case "[":
                        Token openBracket = tokens.Pop();
                        Expression index = this.ParseExpression(this.activeOwner);
                        Token closeBracket = tokens.PopExpected("]");
                        root = new BracketIndex(root, openBracket, index);
                        break;
                    case "(":
                        Token openParen = tokens.Pop();
                        List<Expression> argList = new List<Expression>();
                        while (!tokens.PopIfPresent(")"))
                        {
                            if (argList.Count > 0) tokens.PopExpected(",");
                            argList.Add(this.ParseExpression(this.activeOwner));
                        }
                        root = new FunctionInvocation(root, openParen, argList);
                        break;
                    case "as":
                        Token asToken = tokens.Pop();
                        Type targetType = this.parser.TypeParser.ParseType();
                        root = new NullableCast(root, asToken, targetType);
                        break;
                    default:
                        parseSuffixes = false;
                        break;
                }
            }
            return root;
        }

        private IntegerConstant ParseHexConstant(Token token)
        {
            string str = token.Value.ToLower();
            if (str.StartsWith("0x") && str.Length > 2)
            {
                bool valid = true;
                int value = 0;
                for (int i = 2; i < str.Length; i++)
                {
                    value = value * 16;
                    char c = str[i];
                    if (c >= '0' && c <= '9')
                    {
                        value += c - '0';
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        value += c - 'a' + 10;
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) return new IntegerConstant(token, value);
            }

            throw new ParserException(token, "Invalid hexadecimal constant: '" + token.Value + "'");
        }

        private Expression ParseAtomic()
        {
            tokens.EnsureNotEof();
            Token nextToken = tokens.Peek();
            string next = nextToken.Value;
            switch (next)
            {
                case "true": return new BooleanConstant(tokens.Pop(), true);
                case "false": return new BooleanConstant(tokens.Pop(), false);
                case "null": return new NullConstant(tokens.Pop());
                case "this": return new ThisConstant(tokens.Pop(), this.activeOwner);
                case "new": return this.ParseConstructorInvocation();
            }

            switch (nextToken.Type)
            {
                case TokenType.NUMBER:
                    if (next.StartsWith("0x"))
                    {
                        return this.ParseHexConstant(tokens.Pop());
                    }
                    else if (next.Contains("."))
                    {
                        if (double.TryParse(next, out double value))
                        {
                            return new FloatConstant(tokens.Pop(), value);
                        }
                    }
                    else if (next.EndsWith('f') || next.EndsWith('F'))
                    {
                        if (double.TryParse(next.Substring(0, next.Length - 1), out double value))
                        {
                            return new FloatConstant(tokens.Pop(), value);
                        }
                    }
                    else
                    {
                        if (int.TryParse(next, out int value))
                        {
                            return new IntegerConstant(tokens.Pop(), value);
                        }
                    }
                    throw new ParserException(nextToken, "Invalid numeric constant: '" + next + "'");

                case TokenType.STRING:
                    return new StringConstant(tokens.Pop(), this.DecodeString(nextToken, next));

                case TokenType.WORD:
                    return new Variable(tokens.Pop(), next);

                case TokenType.KEYWORD:
                    switch (next)
                    {
                        // TODO: really should just define this list somewhere
                        case "map":
                        case "list":
                        case "array":
                        case "set":
                        case "string":
                        case "int":
                        case "bool":
                        case "void":
                        case "float":
                        case "object":
                            // array<string>.of("<--", "stuff", "like", "that")
                            Type type = this.parser.TypeParser.ParseType();
                            return new TypeRootedExpression(type);
                    }
                    throw new ParserException(tokens.Pop(), "Unexpected token: '" + next + "'");

                case TokenType.PUNC:
                    if (next == "$")
                    {
                        tokens.Pop();
                        Token sysFuncName = tokens.PopNonNull();
                        return new SystemFunction(nextToken, sysFuncName);
                    }
                    throw new ParserException(tokens.Pop(), "Unexpected token: '" + next + "'");

                default:
                    throw new ParserException(tokens.Pop(), "Unexpected token: '" + next + "'");
            }
        }

        private string DecodeString(Token throwToken, string rawValue)
        {
            rawValue = rawValue.Substring(1, rawValue.Length - 2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rawValue.Length; i++)
            {
                char c = rawValue[i];
                if (c == '\\')
                {
                    if (i == rawValue.Length - 1) throw new ParserException(throwToken, "String cannot have a \\ character at the end without an escape sequence.");
                    c = rawValue[i + 1];
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        case '\0': sb.Append('\0'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append("'"); break;
                        // TODO: unicode
                        default:
                            throw new ParserException(throwToken, "Unrecognized string escape sequence: '\\" + c + "'");
                    }
                    i++;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private Expression ParseConstructorInvocation()
        {
            Token newToken = tokens.PopExpected("new");
            Token classNameToken = tokens.PopNonNull();
            if (classNameToken.Type != TokenType.WORD) throw new ParserException(classNameToken, "Expected class name");
            tokens.PopExpected("(");
            List<Expression> args = new List<Expression>();
            while (!tokens.PopIfPresent(")"))
            {
                if (args.Count > 0) tokens.PopExpected(",");
                args.Add(this.ParseExpression(this.activeOwner));
            }
            return new ConstructorInvocation(newToken, classNameToken, args);
        }

        private class OpChainParser
        {
            public Func<TokenStream, Expression> NextParser { get; set; }
            private HashSet<string> ops;

            public OpChainParser(ICollection<string> ops)
            {
                this.ops = new HashSet<string>(ops);
            }

            public Expression Parse(TokenStream tokens)
            {
                Expression expr = this.NextParser(tokens);
                if (tokens.HasMore && this.ops.Contains(tokens.PeekValue()))
                {
                    List<Expression> expressions = new List<Expression>() { expr };
                    List<Token> ops = new List<Token>();
                    while (tokens.HasMore && this.ops.Contains(tokens.PeekValue()))
                    {
                        ops.Add(tokens.Pop());
                        expressions.Add(this.NextParser(tokens));
                    }
                    expr = new OpChain(expressions, ops);
                }
                return expr;
            }
        }
    }
}
