using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ForRangeLoop : Executable
    {
        public VariableDeclaration VarDeclaration { get; private set; }
        public Expression StartExpression { get; private set; }
        public Expression EndExpression { get; private set; }
        public bool IsInclusive { get; private set; }
        public Executable[] Code { get; private set; }

        public ForRangeLoop(Token forToken, TopLevelEntity owner, VariableDeclaration varDecl, Expression startExpr, Expression endExpr, bool isInclusive, IList<Executable> code)
            : base(forToken, owner)
        {
            this.VarDeclaration = varDecl;
            this.StartExpression = startExpr;
            this.EndExpression = endExpr;
            this.IsInclusive = isInclusive;
            this.Code = code.ToArray();
        }

        public override void ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.StartExpression = this.StartExpression.ResolveVariables(resolver, scope);
            this.EndExpression = this.EndExpression.ResolveVariables(resolver, scope);
            LexicalScope outerScope = new LexicalScope(scope);
            outerScope.AddDefinition(this.VarDeclaration);
            LexicalScope innerScope = new LexicalScope(outerScope);
            foreach (Executable line in this.Code)
            {
                line.ResolveVariables(resolver, innerScope);
            }
        }

        public override void ResolveTypes(Resolver resolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
