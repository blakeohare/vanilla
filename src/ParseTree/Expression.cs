namespace Vanilla.ParseTree
{
    internal abstract class Expression : Entity
    {
        public Type ResolvedType { get; set; }
        public Expression(Token firstToken) : base(firstToken) { }

        public abstract Expression ResolveVariables(Resolver resolver, LexicalScope scope);
        public abstract Expression ResolveTypes(Resolver resolver);
    }
}
