using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class SwitchChunk
    {
        public Token[] CaseTokens { get; private set; }
        public Expression[] CaseValues { get; private set; }
        public Executable[] Code { get; private set; }

        public SwitchChunk(IList<Token> caseTokens, IList<Expression> caseValues, IList<Executable> code)
        {
            this.CaseTokens = caseTokens.ToArray();
            this.CaseValues = caseValues.ToArray();
            this.Code = code.ToArray();

            if (this.CaseTokens.Length == 0) throw new Exception(); // should not happen
            for (int i = 0; i < this.CaseValues.Length - 1; i++)
            {
                if (this.CaseValues[i] == null) throw new ParserException(caseTokens[i], "A switch case can only have one 'default' and it must come at the end.");
            }
            if (this.Code.Length == 0) throw new ParserException(this.CaseTokens[0], "This case is empty.");
        }
    }
}
