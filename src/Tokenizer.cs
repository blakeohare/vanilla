using System;
using System.Collections.Generic;

namespace Vanilla
{
    internal class Tokenizer
    {
        private enum State
        {
            NORMAL,
            STRING,
            COMMENT,
            WORD,
            DIRECTIVE,
        }

        private static HashSet<string> TWO_CHAR_TOKENS = new HashSet<string>() {
            "++", "--",
            "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
            "<<", ">>",
            "&&", "||",
            "!=", "==", "<=", ">=",
        };

        private static HashSet<string> THREE_CHAR_TOKENS = new HashSet<string>()
        {
            "<<=", ">>=",
        };

        private static HashSet<string> KEYWORDS = new HashSet<string>()
        {
            "array",
            "base",
            "bool",
            "break",
            "class",
            "const",
            "constructor",
            "continue",
            "double",
            "else",
            "false",
            "field",
            "float",
            "for",
            "func",
            "function",
            "if",
            "int",
            "interface",
            "list",
            "map",
            "new",
            "null",
            "object",
            "return",
            "set",
            "static",
            "string",
            "this",
            "true",
            "var",
            "void",
            "while",
        };

        private static TokenType IdentifyType(string word)
        {
            char c = word[0];
            if (c >= '0' && c <= '9') return TokenType.NUMBER;
            if (KEYWORDS.Contains(word)) return TokenType.KEYWORD;
            return TokenType.WORD;
        }

        public static TokenStream Tokenize(string filename)
        {
            string absolutePath = FileUtil.NormalizeToAbsolute(filename);
            string content = System.IO.File.ReadAllText(absolutePath);
            if (content == null) throw new InvalidOperationException("File does not exist: " + absolutePath);

            List<Token> tokens = new List<Token>();
            TokenizeImpl(tokens, filename, content);

            // consolidate numbers into a single token in the pattern of 123.456, 123. and .123
            // Tokens before this point are considered a number if they start with a number so things like 0x123abc and 456F or 1357notanumber all count as a number.
            // If they are a bad format, then that is caught downstream.
            List<Token> consolidated = new List<Token>();

            Token fakeToken = new Token() { Type = TokenType.KEYWORD, Line = content.Length + 999, Col = 0, Value = "false" };
            for (int i = 0; i < tokens.Count; i++)
            {
                Token t1 = tokens[i];
                Token t2 = i + 1 < tokens.Count ? tokens[i + 1] : fakeToken;
                Token t3 = i + 2 < tokens.Count ? tokens[i + 2] : fakeToken;

                bool match1 = t1.Line == t2.Line && t1.Col + t1.Value.Length == t2.Col && t1.FileName == t2.FileName;
                bool match2 = t2.Line == t3.Line && t2.Col + t2.Value.Length == t3.Col && t2.FileName == t3.FileName;

                if (t1.Type == TokenType.NUMBER && t2.Value == "." && t3.Type == TokenType.NUMBER && match1 && match2)
                {
                    t1.Value += "." + t3.Value;
                    i += 2;
                }
                else if (t1.Value == "." && t2.Type == TokenType.NUMBER && match1)
                {
                    t1.Value += t2.Value;
                    t1.Type = TokenType.NUMBER;
                    i += 1;
                }
                else if (t1.Type == TokenType.NUMBER && t2.Value == "." && match1)
                {
                    t1.Value += ".";
                    i += 1;
                }
                consolidated.Add(t1);
            }

            return new TokenStream(consolidated);
        }

        private static void TokenizeImpl(List<Token> tokens, string filename, string content)
        {
            content = content.TrimEnd().Replace("\r\n", "\n").Replace('\r', '\n') + "\n";

            int length = content.Length;
            int[] lines = new int[length];
            int[] cols = new int[length];
            int line = 1;
            int col = 1;
            char c;
            for (int i = 0; i < length; i++)
            {
                lines[i] = line;
                cols[i] = col;
                c = content[i];
                if (c == '\n')
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col++;
                }
            }

            State state = State.NORMAL;
            char c2;
            int tokenStart = 0;
            char tokenSubType = ' ';
            for (int i = 0; i < length; i++)
            {
                c = content[i];
                c2 = i + 1 < length ? content[i + 1] : '\0';
                switch (state)
                {
                    case State.NORMAL:
                        if ((c >= 'A' && c <= 'Z') ||
                            (c >= 'a' && c <= 'z') ||
                            (c >= '0' && c <= '9') ||
                            c == '_')
                        {
                            tokenStart = i;
                            state = State.WORD;
                        }
                        else if (c == '"' || c == '\'')
                        {
                            tokenStart = i;
                            state = State.STRING;
                            tokenSubType = c;
                        }
                        else if (c == '/' && (c2 == '/' || c2 == '*'))
                        {
                            state = State.COMMENT;
                            tokenSubType = c2;
                            i++;
                        }
                        else if (c == ' ' || c == '\t' || c == '\n')
                        {
                            // whitespace
                        }
                        else
                        {
                            if (c == '#' && (i == 0 || content[i - 1] == '\n'))
                            {
                                tokenStart = i;
                                state = State.DIRECTIVE;
                            }
                            else
                            {

                                string value = c + "";
                                string value2 = value + c2;
                                if (TWO_CHAR_TOKENS.Contains(value2))
                                {
                                    value = value2;
                                    if (i + 2 < length && THREE_CHAR_TOKENS.Contains(value2 + content[i + 2]))
                                    {
                                        value += content[i + 2];
                                    }
                                }
                                tokens.Add(new Token() { FileName = filename, Value = value, Type = TokenType.PUNC, Line = lines[i], Col = cols[i] });
                                i += value.Length - 1;
                            }
                        }
                        break;

                    case State.DIRECTIVE:
                        if (c == '\n')
                        {
                            string directive = content.Substring(tokenStart, i - tokenStart);
                            HandleDirective(directive, filename, tokens);
                            state = State.NORMAL;
                        }
                        break;

                    case State.COMMENT:
                        if (tokenSubType == '/' && c == '\n')
                        {
                            state = State.NORMAL;
                        }
                        else if (tokenSubType == '*' && c == '*' && c2 == '/')
                        {
                            state = State.NORMAL;
                            i++;
                        }
                        break;

                    case State.WORD:
                        if ((c >= 'A' && c <= 'Z') ||
                            (c >= 'a' && c <= 'z') ||
                            (c >= '0' && c <= '9') ||
                            c == '_')
                        {
                            // still in a word
                        }
                        else
                        {
                            string value = content.Substring(tokenStart, i - tokenStart);
                            tokens.Add(new Token() { FileName = filename, Value = value, Type = IdentifyType(value), Line = lines[i], Col = cols[tokenStart] });
                            state = State.NORMAL;
                            --i;
                        }
                        break;

                    case State.STRING:
                        if (c == '\\')
                        {
                            i++; // uncoditionally skip the next character. Valid escape sequences checked downstream.
                        }
                        else if (c == tokenSubType)
                        {
                            tokens.Add(new Token() { FileName = filename, Value = content.Substring(tokenStart, i - tokenStart + 1), Type = TokenType.STRING, Line = lines[tokenStart], Col = cols[tokenStart] });
                            state = State.NORMAL;
                        }
                        break;
                }
            }

            if (state != State.NORMAL)
            {
                if (state == State.COMMENT) throw new ParserException(filename, "There is an unclosed comment in this file.");
                if (state == State.STRING) throw new ParserException(filename, "There is an unclosed string in this file.");
                throw new Exception(); // this should not happen.
            }
        }

        private static void HandleDirective(string directiveText, string currentFilename, List<Token> tokens)
        {
            if (directiveText.StartsWith("#include"))
            {
                string includePath = directiveText.Substring("#include".Length).Trim();
                string newFile = FileUtil.NormalizeToAbsolute(currentFilename + "/../" + includePath);
                string fileContent = FileUtil.FileReadText(newFile);
                if (fileContent == null) throw new InvalidOperationException("File does not exist: " + newFile);
                TokenizeImpl(tokens, newFile, fileContent);
            }
        }
    }
}
