using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ForLoop : Executable
    {
        public Executable[] Init { get; private set; }
        public Expression Condition { get; private set; }
        public Executable[] Step { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(Token forToken, TopLevelEntity owner, IList<Executable> init, Expression condition, IList<Executable> step, IList<Executable> code)
            : base(forToken, owner)
        {
            this.Init = init.ToArray();
            this.Condition = condition;
            this.Step = step.ToArray();
            this.Code = code.ToArray();
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            LexicalScope outerScope = new LexicalScope(scope);
            foreach (Executable initItem in this.Init)
            {
                initItem.ResolveVariables(resolver, outerScope);
            }
            if (this.Condition != null)
            {
                this.Condition = this.Condition.ResolveVariables(resolver, outerScope);
            }
            foreach (Executable stepItem in this.Step)
            {
                stepItem.ResolveVariables(resolver, outerScope);
            }
            LexicalScope innerScope = new LexicalScope(outerScope);
            foreach (Executable line in this.Code)
            {
                line.ResolveVariables(resolver, innerScope);
            }
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
