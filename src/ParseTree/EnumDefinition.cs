using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class EnumDefinition : TopLevelEntity
    {
        public Token NameToken { get; private set; }
        public Token[] MemberNameTokens { get; private set; }
        public string[] MemberNames { get; private set; }
        public Expression[] MemberValues { get; private set; }
        public int[] MemberCalculatedValues { get; private set; }

        public EnumDefinition(Token enumToken, Token nameToken, IList<Token> memberNames, IList<Expression> memberValues)
            : base(enumToken)
        {
            this.NameToken = nameToken;
            this.MemberNameTokens = memberNames.ToArray();
            this.MemberNames = memberNames.Select(n => n.Value).ToArray();
            this.MemberValues = memberValues.ToArray();
        }
    }
}
