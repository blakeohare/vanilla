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

        private Dictionary<string, TopLevelEntity> directMemberLookup = null;

        public ClassDefinition(Token classToken, Token nameToken) : base(classToken)
        {
            this.ClassToken = classToken;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
        }

        public void SetMembers(IList<TopLevelEntity> members)
        {
            this.directMemberLookup = null;
            this.Members = members.ToArray();
        }

        public TopLevelEntity GetMemberWithInheritance(string name)
        {
            // TODO: base classes
            if (this.directMemberLookup == null)
            {
                this.directMemberLookup = new Dictionary<string, TopLevelEntity>();
                foreach (TopLevelEntity tle in this.Members)
                {
                    if (tle is FunctionDefinition)
                    {
                        this.directMemberLookup[((FunctionDefinition)tle).Name] = tle;
                    }
                    else if (tle is Field)
                    {
                        this.directMemberLookup[((Field)tle).Name] = tle;
                    }
                    else if (tle is ConstructorDefinition)
                    {
                        // skip
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }

            if (this.directMemberLookup.ContainsKey(name))
            {
                return this.directMemberLookup[name];
            }

            // TODO: check base class

            return null;
        }
    }
}
