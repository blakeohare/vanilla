using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class SwitchStatement : Executable
    {
        public Expression Condition { get; private set; }
        public SwitchChunk[] Chunks { get; private set; }

        public SwitchStatement(TopLevelEntity owner, Token switchToken, Expression condition, IList<SwitchChunk> chunks) : base(switchToken, owner)
        {
            this.Condition = condition;
            this.Chunks = chunks.ToArray();
        }
    }
}
