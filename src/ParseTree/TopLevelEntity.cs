namespace Vanilla.ParseTree
{
    internal class TopLevelEntity : Entity
    {
        public ClassDefinition WrapperClass { get; set; }

        public TopLevelEntity(Token firstToken) : base(firstToken)
        { }
    }
}
