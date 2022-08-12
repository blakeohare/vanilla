using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class EnumDefinition : TopLevelEntity
    {
        public Token NameToken { get; private set; }
        public Token[] MemberNameTokens { get; set; }
        public string[] MemberNames { get { return this.MemberNameTokens.Select(n => n.Value).ToArray(); } }
        public Expression[] MemberValues { get; set; }
        public int[] MemberCalculatedValues { get; private set; }

        public EnumDefinition(Token enumToken, Token nameToken)
            : base(enumToken)
        {
            this.NameToken = nameToken;
        }
    }
}
