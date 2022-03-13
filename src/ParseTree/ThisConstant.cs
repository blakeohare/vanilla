﻿namespace Vanilla.ParseTree
{
    internal class ThisConstant : Expression
    {
        public ThisConstant(Token thisToken) : base(thisToken) { }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            return this;
        }
    }
}
