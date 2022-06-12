using System;
using System.Collections.Generic;
using System.Linq;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class ExecutableParser
    {
        private Parser parser;
        private TokenStream tokens;
        public EntityParser EntParser { get { return this.parser.EntParser; } }
        public ExecutableParser ExecParser { get { return this.parser.ExecParser; } }
        public ExpressionParser ExprParser { get { return this.parser.ExprParser; } }

        private static readonly Executable[] EMPTY_CODE_BLOCK = new Executable[0];

        public ExecutableParser(Parser parser, TokenStream tokens)
        {
            this.parser = parser;
            this.tokens = tokens;
        }

        public Executable[] ParseCodeBlock(TopLevelEntity owner, bool openBraceRequired)
        {
            bool openBracePresent = tokens.PopIfPresent("{");
            if (!openBracePresent && openBraceRequired) tokens.PopExpected("{"); // throws
            List<Executable> lines = new List<Executable>();
            if (openBracePresent)
            {
                while (!tokens.PopIfPresent("}"))
                {
                    if (!tokens.PopIfPresent(";"))
                    {
                        lines.Add(this.ParseExecutable(owner, true, true));
                    }
                }
            }
            else
            {
                if (!tokens.PopIfPresent(";"))
                {
                    lines.Add(this.ParseExecutable(owner, true, true));
                }
            }

            return lines.ToArray();
        }

        private static readonly HashSet<string> ASSIGN_OPS = new HashSet<string>("= += -= *= /= %= &= |- ^= >>= <<=".Split(' '));

        public Executable ParseExecutable(TopLevelEntity owner, bool allowComplex, bool popSemicolon)
        {
            tokens.EnsureNotEof();
            string next = tokens.PeekValue();
            if (allowComplex)
            {
                switch (next)
                {
                    case "for": return this.ParseFor(owner);
                    case "while": return this.ParseWhile(owner);
                    case "if": return this.ParseIf(owner);
                    case "switch": return this.ParseSwitch(owner);
                    case "return": return this.ParseReturn(owner);
                    case "break": return this.ParseBreak(owner);
                    case "continue": return this.ParseContinue(owner);
                }
            }

            if (next == "const" || next == "var")
            {
                return this.ParseVariableDeclaration(owner, popSemicolon);
            }

            Executable ex;
            Expression expr = this.ExprParser.ParseExpression();
            tokens.EnsureNotEof();
            next = tokens.PeekValue();
            if (ASSIGN_OPS.Contains(next))
            {
                Token assignToken = tokens.Pop();
                Expression targetValue = this.ExprParser.ParseExpression();
                ex = new Assignment(expr, assignToken, targetValue, owner);
            }
            else
            {
                ex = new ExpressionAsExecutable(owner, expr);
            }

            if (popSemicolon)
            {
                tokens.PopExpected(";");
            }

            return ex;
        }

        private enum ForLoopType
        {
            TRADITIONAL,
            FOREACH,
            RANGE,
        }

        private Executable ParseFor(TopLevelEntity owner)
        {
            Token forToken = tokens.PopExpected("for");
            tokens.PopExpected("(");

            List<Executable> init = new List<Executable>();
            ForLoopType forType = this.DetermineForLoopType(owner, init);
            Executable[] code;
            VariableDeclaration loopVarDecl = init.FirstOrDefault() as VariableDeclaration;
            switch (forType)
            {
                case ForLoopType.FOREACH:
                    if (loopVarDecl == null) throw new ParserException(forToken, "for loops over collections require a variable declaration");
                    if (!loopVarDecl.IsConst) throw new ParserException(loopVarDecl, "for loop iterating variables must be constants");
                    tokens.PopExpected("in");
                    Expression collection = this.ExprParser.ParseExpression();
                    tokens.PopExpected(")");
                    code = this.ExecParser.ParseCodeBlock(owner, false);
                    return new ForEachLoop(forToken, owner, loopVarDecl, collection, code);

                case ForLoopType.RANGE:
                    if (loopVarDecl == null) throw new ParserException(forToken, "for loops over ranges require a variable declaration");
                    if (!loopVarDecl.IsConst) throw new ParserException(loopVarDecl, "for loop iterating variables must be constants");
                    tokens.PopExpected("from");
                    Expression start = this.ExprParser.ParseExpression();
                    if (!tokens.IsNext("thru") && !tokens.IsNext("till")) tokens.PopExpected("till"); // throws
                    Token endQualifier = tokens.Pop();
                    Expression end = this.ExprParser.ParseExpression();
                    tokens.PopExpected(")");
                    code = this.ExecParser.ParseCodeBlock(owner, false);
                    return new ForRangeLoop(forToken, owner, loopVarDecl, start, end, endQualifier.Value == "thru", code);

                case ForLoopType.TRADITIONAL:
                    tokens.PopExpected(";");
                    Expression condition = null;
                    if (!tokens.PopIfPresent(";"))
                    {
                        condition = this.ExprParser.ParseExpression();
                        tokens.PopExpected(";");
                    }
                    List<Executable> step = new List<Executable>();
                    if (!tokens.IsNext(")"))
                    {
                        step.Add(this.ParseExecutable(owner, false, false));
                        while (tokens.PopIfPresent(","))
                        {
                            step.Add(this.ParseExecutable(owner, false, false));
                        }
                    }
                    tokens.PopExpected(")");
                    code = this.ExecParser.ParseCodeBlock(owner, false);
                    return new ForLoop(forToken, owner, init, condition, step, code);

                default: throw new Exception();
            }

            throw new NotImplementedException();
        }

        private ForLoopType DetermineForLoopType(TopLevelEntity owner, List<Executable> initOut)
        {
            tokens.EnsureNotEof();
            if (tokens.IsNext(";")) return ForLoopType.TRADITIONAL;
            Executable firstExec = this.ExecParser.ParseExecutable(owner, false, false);
            initOut.Add(firstExec);
            if (tokens.IsNext(";") || tokens.IsNext(","))
            {
                while (tokens.PopIfPresent(","))
                {
                    initOut.Add(this.ExecParser.ParseExecutable(owner, false, false));
                }
                return ForLoopType.TRADITIONAL;
            }

            if (tokens.IsNext("from")) return ForLoopType.RANGE;
            if (tokens.IsNext("in")) return ForLoopType.FOREACH;

            throw new ParserException(firstExec.FirstToken, "This is an invalid for loop.");
        }

        private Executable ParseWhile(TopLevelEntity owner)
        {
            throw new NotImplementedException();
        }

        private Executable ParseIf(TopLevelEntity owner)
        {
            Token ifToken = tokens.PopExpected("if");
            tokens.PopExpected("(");
            Expression condition = this.ExprParser.ParseExpression();
            tokens.PopExpected(")");
            Executable[] trueCode = this.ExecParser.ParseCodeBlock(owner, false);
            Executable[] falseCode = EMPTY_CODE_BLOCK;
            if (tokens.IsNext("else"))
            {
                tokens.PopExpected("else");
                falseCode = this.ExecParser.ParseCodeBlock(owner, false);
            }
            return new IfStatement(owner, ifToken, condition, trueCode, falseCode);
        }

        private Executable ParseSwitch(TopLevelEntity owner)
        {
            Token switchToken = tokens.PopExpected("switch");
            tokens.PopExpected("(");
            Expression condition = this.ExprParser.ParseExpression();
            tokens.PopExpected(")");
            tokens.PopExpected("{");
            List<SwitchChunk> chunks = new List<SwitchChunk>();
            List<Expression> activeChunkCases = new List<Expression>();
            List<Token> caseTokens = new List<Token>();
            List<Executable> activeChunkCode = new List<Executable>();
            bool isParseCaseMode = true;

            while (!tokens.PopIfPresent("}"))
            {
                bool isCaseNext = tokens.IsNext("case");
                bool isDefaultNext = tokens.IsNext("default");
                if (isParseCaseMode)
                {
                    if (isCaseNext)
                    {
                        caseTokens.Add(tokens.PopExpected("case"));
                        activeChunkCases.Add(this.ExprParser.ParseExpression());
                        tokens.PopExpected(":");
                    }
                    else if (isDefaultNext)
                    {
                        caseTokens.Add(tokens.PopExpected("default"));
                        tokens.PopExpected(":");
                    }
                    else
                    {
                        if (caseTokens.Count == 0) tokens.PopExpected("case"); // throws when code is placed directly inside a switch without a case/default
                        isParseCaseMode = false;
                    }
                }
                else
                {
                    if (isCaseNext || isDefaultNext)
                    {
                        isParseCaseMode = true;
                        chunks.Add(new SwitchChunk(caseTokens, activeChunkCases, activeChunkCode));
                        caseTokens.Clear();
                        activeChunkCases.Clear();
                        activeChunkCode.Clear();
                    }
                    else
                    {
                        activeChunkCode.Add(this.ExecParser.ParseExecutable(owner, true, true));
                    }
                }
            }

            chunks.Add(new SwitchChunk(caseTokens, activeChunkCases, activeChunkCode));

            return new SwitchStatement(owner, switchToken, condition, chunks);
        }

        private Executable ParseReturn(TopLevelEntity owner)
        {
            Token returnToken = tokens.PopExpected("return");
            if (tokens.PopIfPresent(";"))
            {
                return new ReturnStatement(returnToken, null, owner);
            }
            Expression value = this.ExprParser.ParseExpression();
            tokens.PopExpected(";");

            return new ReturnStatement(returnToken, value, owner);
        }

        private Executable ParseBreak(TopLevelEntity owner)
        {
            Token breakToken = tokens.PopExpected("break");
            tokens.PopExpected(";");
            return new BreakStatement(breakToken, owner);
        }

        private Executable ParseContinue(TopLevelEntity owner)
        {
            throw new NotImplementedException();
        }

        private Executable ParseVariableDeclaration(TopLevelEntity owner, bool popSemicolon)
        {
            Token declaration = tokens.PopNonNull();
            if (declaration.Value != "const" && declaration.Value != "var") tokens.PopExpected("const"); // throws
            tokens.PopExpected(":");
            Type varType = this.parser.TypeParser.ParseType();
            Token varName = tokens.PopNonNull();
            if (varName.Type != TokenType.WORD) throw new ParserException(varName, "Invalid variable name: '" + varName.Value + "'");

            Expression initialValue = null;
            Token assignmentOp = null;
            if (tokens.IsNext("="))
            {
                assignmentOp = tokens.PopExpected("=");
                initialValue = this.ExprParser.ParseExpression();
            }
            if (popSemicolon)
            {
                tokens.PopExpected(";");
            }
            return new VariableDeclaration(owner, declaration, varType, varName, assignmentOp, initialValue);
        }
    }
}
