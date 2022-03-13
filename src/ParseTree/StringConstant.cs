﻿namespace Vanilla.ParseTree
{
    internal class StringConstant : Expression
    {
        public string Value { get; private set; }

        public StringConstant(Token token, string value) : base(token)
        {
            this.Value = value;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
