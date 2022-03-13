using System.Collections.Generic;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class LexicalScope
    {
        private Dictionary<string, VariableDeclaration> vars = new Dictionary<string, VariableDeclaration>();

        private LexicalScope parent;

        public LexicalScope(LexicalScope optionalParent)
        {
            this.parent = optionalParent;
        }

        public void AddDefinition(VariableDeclaration varDecl)
        {
            string name = varDecl.Name;
            if (this.vars.ContainsKey(name)) throw new ParserException(varDecl.FirstToken, "There are multiple definitions of the variable '" + name + "' in the same scope.");
            this.vars[name] = varDecl;
        }

        public VariableDeclaration TryGetDeclaration(string name)
        {
            if (this.vars.ContainsKey(name)) return this.vars[name];
            if (this.parent != null)
            {
                return this.parent.TryGetDeclaration(name);
            }
            return null;
        }
    }
}
