using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ClassDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public int FlattenedMemberCount { get { return this.Members.Length; } }
        public ConstructorDefinition Constructor { get; private set; }

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

            ConstructorDefinition[] ctors = this.Members.OfType<ConstructorDefinition>().ToArray();
            if (ctors.Length != 1)
            {
                if (ctors.Length == 0) throw new ParserException(this, "The class '" + this.Name + "' is missing a constructor.");
                throw new ParserException(this, "The class '" + this.Name + "' has multiple constructors.");
            }
            ConstructorDefinition ctor = ctors[0];
            this.Constructor = ctor;
        }

        private Dictionary<string, int> memberOffsetId = null;
        public int GetMemberOffsetId(string name)
        {
            // TODO: check base class
            if (memberOffsetId == null)
            {
                Field[] fields = this.Members.OfType<Field>().OrderBy(f => f.Name).ToArray();
                FunctionDefinition[] funcs = this.Members.OfType<FunctionDefinition>().OrderBy(fn => fn.Name).ToArray();
                int id = 2;
                this.memberOffsetId = new Dictionary<string, int>();
                foreach (Field f in fields)
                {
                    this.memberOffsetId[f.Name] = id++;
                }
                foreach (FunctionDefinition fn in funcs)
                {
                    this.memberOffsetId[fn.Name] = id++;
                }
            }
            if (memberOffsetId.ContainsKey(name)) throw new System.Exception(); // should not happen.
            return memberOffsetId[name];
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
