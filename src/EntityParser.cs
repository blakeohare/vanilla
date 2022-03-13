using System;
using System.Collections.Generic;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class EntityParser
    {
        private Parser parser;
        private TokenStream tokens;
        public EntityParser EntParser { get { return this.parser.EntParser; } }
        public ExecutableParser ExecParser { get { return this.parser.ExecParser; } }
        public ExpressionParser ExprParser { get { return this.parser.ExprParser; } }
        public TypeParser TypeParser { get { return this.parser.TypeParser; } }

        public EntityParser(Parser parser, TokenStream tokens)
        {
            this.parser = parser;
            this.tokens = tokens;
        }

        private Modifiers ParseModifiersIfPresent()
        {
            string next = tokens.PeekValue();
            if (next == "@")
            {
                Token modFirst = tokens.Peek();
                List<Token> modifierList = new List<Token>();
                while (next == "@")
                {
                    // TODO: consider consolidating these at tokenization time.
                    Token atToken = tokens.PopExpected("@");
                    Token modifierName = tokens.Pop();
                    if (!modifierName.IsImmediatelyAfter(atToken)) throw new ParserException(atToken, "Unexpected token: '@'");
                    if (modifierName.Type != TokenType.WORD) throw new ParserException(modifierName, "Expected a modifier");
                    modifierList.Add(modifierName);
                    next = tokens.PeekValue();
                }
                return new Modifiers(modFirst, modifierList);
            }
            return null;
        }

        public TopLevelEntity ParseEntity()
        {
            Modifiers modifiers = this.ParseModifiersIfPresent();

            string next = tokens.PeekValue();

            switch (next)
            {
                case "class": return this.ParseClass();
                case "function": return this.ParseFunction(modifiers);
                case "enum": return this.ParseEnum();
                case "static": return this.ParseField();
                default: throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        private TopLevelEntity ParseClass()
        {
            Token classToken = tokens.PopExpected("class");
            Token nameToken = tokens.PopNonNull();
            if (nameToken.Type != TokenType.WORD) throw new ParserException(nameToken, "Expected a class name");
            tokens.PopExpected("{");

            ClassDefinition cd = new ClassDefinition(classToken, nameToken);

            List<TopLevelEntity> members = new List<TopLevelEntity>();

            while (!tokens.IsNext("}") && tokens.HasMore)
            {
                TopLevelEntity mem = this.ParseClassMember(cd);
                members.Add(mem);
            }

            tokens.PopExpected("}");

            cd.SetMembers(members);

            return cd;
        }

        private TopLevelEntity ParseClassMember(ClassDefinition cd)
        {
            string next = tokens.PeekValue();
            switch (next)
            {
                case "field": return this.ParseField();
                case "constructor": return this.ParseConstructor();
                case "function": return this.ParseFunction(null); // modifiers not allowed on methods
                default: throw new NotImplementedException();
            }
        }

        private TopLevelEntity ParseField()
        {
            if (!tokens.IsNext("field") && !tokens.IsNext("static")) tokens.PopExpected("field"); // throws
            Token fieldToken = tokens.Pop();
            tokens.PopExpected(":");
            Type type = this.TypeParser.ParseType();

            Token fieldName = tokens.PopNonNull();
            if (fieldName.Type != TokenType.WORD) throw new ParserException(fieldName, "Expected field name");

            Expression startingValue = null;
            if (!tokens.PopIfPresent(";"))
            {
                tokens.PopExpected("=");
                startingValue = this.ExprParser.ParseExpression();
                tokens.PopExpected(";");
            }

            return new Field(fieldToken, type, fieldName, startingValue);
        }

        private TopLevelEntity ParseConstructor()
        {
            Token constructorToken = tokens.PopExpected("constructor");
            ArgumentList args = ArgumentList.Parse(tokens, this.TypeParser);
            Token baseToken = null;
            List<Expression> baseArgs = new List<Expression>();
            if (tokens.PopIfPresent(":"))
            {
                baseToken = tokens.PopExpected("base");
                tokens.PopExpected("(");
                baseArgs = new List<Expression>();
                while (!tokens.PopIfPresent(")"))
                {
                    if (baseArgs.Count > 0) tokens.PopExpected(",");
                    baseArgs.Add(this.ExprParser.ParseExpression());
                }
            }

            ConstructorDefinition cd = new ConstructorDefinition(constructorToken, args.ArgDeclarations, args.ArgTypes, args.ArgNames, baseToken, baseArgs);

            cd.Body = this.ExecParser.ParseCodeBlock(cd, true);

            return cd;
        }

        private TopLevelEntity ParseFunction(Modifiers modifiers)
        {
            Token functionToken = tokens.PopExpected("function");
            tokens.PopExpected(":");
            Type returnType = this.TypeParser.ParseType();
            Token functionName = tokens.PopNonNull();
            if (functionName.Type != TokenType.WORD) throw new ParserException(functionName, "Expected function name");
            ArgumentList args = ArgumentList.Parse(tokens, this.TypeParser);
            FunctionDefinition fd = new FunctionDefinition(modifiers, functionToken, returnType, functionName, args.ArgDeclarations, args.ArgTypes, args.ArgNames);
            fd.Body = this.ExecParser.ParseCodeBlock(fd, true);
            return fd;
        }

        private TopLevelEntity ParseEnum()
        {
            Token enumToken = tokens.PopExpected("enum");
            Token nameToken = tokens.PopNonNull();
            if (nameToken.Type != TokenType.WORD) throw new ParserException(nameToken, "Invalid enum name.");
            tokens.PopExpected("{");
            bool nextAllowed = true;
            List<Token> memberNames = new List<Token>();
            List<Expression> memberValues = new List<Expression>();

            while (!tokens.PopIfPresent("}"))
            {
                if (!nextAllowed) tokens.PopExpected("}"); // throws
                Token memberName = tokens.PopNonNull();
                if (memberName.Type != TokenType.WORD) throw new ParserException(memberName, "Invalid enum member name.");
                if (memberName.Value != memberName.Value.ToUpper()) throw new ParserException(memberName, "Enum members must be in ALL-CAPS");
                Expression memberValue = null;
                if (tokens.PopIfPresent("="))
                {
                    memberValue = this.ExprParser.ParseExpression();
                }
                nextAllowed = tokens.PopIfPresent(",");

                memberNames.Add(memberName);
                memberValues.Add(memberValue);
            }

            return new EnumDefinition(enumToken, nameToken, memberNames, memberValues);
        }

        private class ArgumentList
        {
            public List<Token> ArgDeclarations { get; private set; }
            public List<Type> ArgTypes { get; private set; }
            public List<Token> ArgNames { get; private set; }
            public int ArgCount { get { return this.ArgDeclarations.Count; } }

            private ArgumentList() { }

            public static ArgumentList Parse(TokenStream tokens, TypeParser typeParser)
            {
                ArgumentList args = new ArgumentList();
                args.ArgDeclarations = new List<Token>();
                args.ArgTypes = new List<Type>();
                args.ArgNames = new List<Token>();

                tokens.PopExpected("(");
                while (!tokens.PopIfPresent(")"))
                {
                    if (args.ArgCount > 0) tokens.PopExpected(",");
                    string next = tokens.PeekValue();
                    if (next != "var" && next != "const") tokens.PopExpected("var"); // throws
                    Token argDecl = tokens.Pop();
                    tokens.PopExpected(":");
                    Type argType = typeParser.ParseType();
                    Token argName = tokens.PopNonNull();
                    if (argName.Type != TokenType.WORD)
                    {
                        throw new ParserException(
                            argName,
                            argName.Type == TokenType.KEYWORD
                                ? "This is a reserved keyword and cannot be used as a function argument name."
                                : "Invalid argument name.");
                    }

                    args.ArgDeclarations.Add(argDecl);
                    args.ArgTypes.Add(argType);
                    args.ArgNames.Add(argName);
                }
                return args;
            }
        }
    }
}
