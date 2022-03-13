namespace Vanilla.ParseTree
{
    internal class ForRangeLoop : Executable
    {
        public VariableDeclaration VarDeclaration { get; private set; }
        public Expression StartExpression { get; private set; }
        public Expression EndExpression { get; private set; }
        public bool IsInclusive { get; private set; }

        public ForRangeLoop(Token forToken, TopLevelEntity owner, VariableDeclaration varDecl, Expression startExpr, Expression endExpr, bool isInclusive)
            : base(forToken, owner)
        {
            this.VarDeclaration = varDecl;
            this.StartExpression = startExpr;
            this.EndExpression = endExpr;
            this.IsInclusive = isInclusive;
        }
    }
}
