namespace Vanilla.ParseTree
{
    internal class Variable : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }

        public Variable(Token nameToken, string name) : base(nameToken)
        {
            this.NameToken = nameToken;
            this.Name = name;
        }
    }
}
