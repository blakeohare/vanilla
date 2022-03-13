using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ClassDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }

        public TopLevelEntity[] Members { get; private set; }

        public ClassDefinition(Token classToken, Token nameToken) : base(classToken)
        {
            this.ClassToken = classToken;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
        }

        public void SetMembers(IList<TopLevelEntity> members)
        {
            this.Members = members.ToArray();
        }
    }
}
