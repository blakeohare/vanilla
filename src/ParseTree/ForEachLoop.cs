using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ForEachLoop : Executable
    {
        public VariableDeclaration VarDeclaration { get; private set; }
        public Expression Collection { get; private set; }
        public Executable[] Code { get; private set; }

        public ForEachLoop(Token forToken, TopLevelEntity owner, VariableDeclaration loopVarDecl, Expression collectionExpr, IList<Executable> code)
            : base(forToken, owner)
        {
            this.VarDeclaration = loopVarDecl;
            this.Collection = collectionExpr;
            this.Code = code.ToArray();
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            throw new System.NotImplementedException();
        }
    }
}
