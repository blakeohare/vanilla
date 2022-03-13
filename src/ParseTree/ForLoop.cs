using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ForLoop : Executable
    {
        public Executable[] Init { get; private set; }
        public Expression Conditionn { get; private set; }
        public Executable[] Step { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(Token forToken, TopLevelEntity owner, IList<Executable> init, Expression condition, IList<Executable> step, IList<Executable> code)
            : base(forToken, owner)
        {
            this.Init = init.ToArray();
            this.Conditionn = condition;
            this.Step = step.ToArray();
            this.Code = code.ToArray();
        }
    }
}
