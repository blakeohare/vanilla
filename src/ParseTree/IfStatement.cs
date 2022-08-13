using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class IfStatement : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] TrueCode { get; private set; }
        public Executable[] FalseCode { get; private set; }

        public IfStatement(TopLevelEntity owner, Token ifToken, Expression condition, IList<Executable> trueCode, IList<Executable> falseCode) : base(ifToken, owner)
        {
            this.Condition = condition;
            this.TrueCode = trueCode.ToArray();
            this.FalseCode = falseCode.ToArray();
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Condition = this.Condition.ResolveVariables(resolver, scope);
            LexicalScope trueScope = new LexicalScope(scope);
            foreach (Executable line in this.TrueCode)
            {
                line.ResolveVariables(resolver, trueScope);
            }
            LexicalScope falseScope = new LexicalScope(scope);
            foreach (Executable line in this.FalseCode)
            {
                line.ResolveVariables(resolver, falseScope);
            }
        }

        public override void ResolveTypes(Resolver resolver)
        {
            this.Condition = this.Condition.ResolveTypes(resolver, null);
            foreach (Executable line in this.TrueCode)
            {
                line.ResolveTypes(resolver);
            }
            foreach (Executable line in this.FalseCode)
            {
                line.ResolveTypes(resolver);
            }
        }
    }
}
