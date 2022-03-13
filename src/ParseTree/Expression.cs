namespace Vanilla.ParseTree
{
    internal abstract class Expression : Entity
    {
        public Expression(Token firstToken) : base(firstToken) { }

        public abstract Expression ResolveVariables(Resolver resolver, LexicalScope scope);
    }
}
