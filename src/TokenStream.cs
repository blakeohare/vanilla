using System.Collections.Generic;
using System.Linq;

namespace Vanilla
{
    internal class TokenStream
    {
        private InternalStream internalStream;

        public TokenStream(IList<Token> tokens)
        {
            this.internalStream = new InternalStream(tokens);
        }

        public Token Peek()
        {
            return this.internalStream.Peek();
        }

        public Token Pop()
        {
            return this.internalStream.Pop();
        }

        public System.Exception ThrowEofException()
        {
            if (this.internalStream.LastToken == null) throw new ParserException("???", "Unexpected End-of-File");
            throw new ParserException(this.internalStream.LastToken, "Unexpected End-of-File");
        }

        public bool HasMore
        {
            get { return this.internalStream.HasNext(); }
        }

        public bool IsNext(string value)
        {
            Token t = this.internalStream.Peek();
            return t != null && t.Value == value;
        }

        public bool PopIfPresent(string value)
        {
            Token t = this.internalStream.Peek();
            if (t != null && t.Value == value)
            {
                this.internalStream.Pop();
                return true;
            }
            return false;
        }

        public void EnsureNotEof()
        {
            if (!this.HasMore) this.ThrowEofException();
        }

        public Token PopNonNull()
        {
            Token t = this.internalStream.Pop();
            if (t == null) throw this.ThrowEofException();
            return t;
        }

        public Token PopExpected(string value)
        {
            Token t = this.internalStream.Pop();
            if (t == null)
            {
                throw this.ThrowEofException();
            }
            if (t.Value != value)
            {
                throw new ParserException(t, "Expected '" + value + "' but found '" + t.Value + "' instead.");
            }
            return t;
        }

        public string PeekValue()
        {
            Token t = this.internalStream.Peek();
            if (t != null) return t.Value;
            return null;
        }

        public void EnableTypeParsingMode()
        {
            this.internalStream.SetMultiCharMode(false);
        }

        public void EnableNormalParsingMode()
        {
            this.internalStream.SetMultiCharMode(true);
        }

        public int SnapshotState()
        {
            return this.internalStream.Snapshot();
        }

        public void RestoreState(int value)
        {
            this.internalStream.Reset(value);
        }

        private class InternalStream
        {
            private Token[] tokens;
            private int index;
            private int length;
            private bool multiCharMode = true;
            private Token next = null;

            public Token LastToken { get { return this.length == 0 ? null : this.tokens[this.length - 1]; } }

            public InternalStream(IList<Token> tokens)
            {
                this.tokens = tokens.ToArray();
                this.index = 0;
                this.length = this.tokens.Length;
            }

            public void SetMultiCharMode(bool isMultiCharMode)
            {
                if (this.multiCharMode != isMultiCharMode)
                {
                    this.multiCharMode = isMultiCharMode;
                    this.next = null;
                }
            }

            public void Reset(int index)
            {
                this.index = index;
                this.next = null;
            }

            public int Snapshot()
            {
                return this.index;
            }

            private Token CalculateNext()
            {
                if (this.index >= this.length) return null;
                Token actualNext = this.tokens[this.index];
                if (!this.multiCharMode)
                {
                    return actualNext;
                }

                if (actualNext.Value == ">" && this.index + 1 < this.length)
                {
                    Token oneAfter = this.tokens[this.index + 1];
                    if (oneAfter.Value == ">" && oneAfter.Col == actualNext.Col + 1 && oneAfter.Line == actualNext.Line)
                    {
                        return new Token()
                        {
                            Value = actualNext.Value,
                            FileName = actualNext.FileName,
                            Type = TokenType.PUNC,
                            Line = actualNext.Line,
                            Col = actualNext.Col,
                            InternalCount = 2,
                        };
                    }
                }
                return actualNext;
            }

            public Token Peek()
            {
                if (this.next != null) return this.next;
                this.next = this.CalculateNext();
                return this.next;
            }

            public Token Pop()
            {
                Token token = this.Peek();
                if (token != null)
                {
                    this.index += token.InternalCount;
                    this.next = null;
                }
                return token;
            }

            public bool HasNext()
            {
                return this.index < this.length;
            }
        }
    }
}
