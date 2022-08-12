namespace Vanilla.ParseTree
{
    internal class Field : TopLevelEntity
    {
        public Type Type { get; private set; }
        public Expression StartingValue { get; private set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public bool IsStatic { get; private set; }

        public Field(Token fieldToken, Type type, Token fieldNameToken, Expression startingValue) : base(fieldToken)
        {
            this.Type = type;
            this.NameToken = fieldNameToken;
            this.Name = this.NameToken.Value;
            this.IsStatic = this.Name == "static";
        }
    }
}
