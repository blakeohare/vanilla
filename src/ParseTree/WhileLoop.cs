using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class WhileLoop : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] Code { get; private set; }

        public WhileLoop(Token whileToken, TopLevelEntity owner, Expression condition, IList<Executable> code)
            : base(whileToken, owner)
        {
            this.Condition = condition;
            this.Code = code.ToArray();
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.Condition = this.Condition.ResolveTypes(resolver, Type.BOOL);

            foreach (Executable line in this.Code)
            {
                line.ResolveTypes(resolver);
            }
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Condition = this.Condition.ResolveVariables(resolver, scope);
            LexicalScope nested = new LexicalScope(scope);

            foreach (Executable line in this.Code)
            {
                line.ResolveVariables(resolver, nested);
            }
        }
    }
}
