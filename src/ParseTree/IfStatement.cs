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
    }
}
