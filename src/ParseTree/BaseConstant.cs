﻿namespace Vanilla.ParseTree
{
    internal class BaseConstant : Expression
    {
        public BaseConstant(Token baseToken) : base(baseToken) { }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }
    }
}
