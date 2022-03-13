namespace Vanilla.ParseTree
{
    internal class Entity
    {
        public Token FirstToken { get; private set; }

        public Entity(Token firstToken)
        {
            this.FirstToken = firstToken;
        }
    }
}
