namespace Vanilla.ParseTree
{
    internal class Executable : Entity
    {
        public TopLevelEntity Owner { get; private set; }

        public Executable(Token firstToken, TopLevelEntity owner) : base(firstToken)
        {
            this.Owner = owner;
        }
    }
}
