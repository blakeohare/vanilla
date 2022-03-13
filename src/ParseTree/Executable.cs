namespace Vanilla.ParseTree
{
    internal abstract class Executable : Entity
    {
        public TopLevelEntity Owner { get; private set; }

        public Executable(Token firstToken, TopLevelEntity owner) : base(firstToken)
        {
            this.Owner = owner;
        }

        public abstract void ResolveVariables(Resolver resolver, LexicalScope scope);
        public abstract void ResolveTypes(Resolver resolver);
    }
}
