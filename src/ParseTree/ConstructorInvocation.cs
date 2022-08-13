using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ConstructorInvocation : Expression
    {
        public Token ClassNameToken { get; private set; }
        public Expression[] Args { get; private set; }

        public ConstructorInvocation(Token newToken, Token classNameToken, IList<Expression> args) : base(newToken)
        {
            this.ClassNameToken = classNameToken;
            this.Args = args.ToArray();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            for (int i = 0; i < this.Args.Length; i++)
            {
                this.Args[i] = this.Args[i].ResolveVariables(resolver, scope);
            }
            return this;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.ResolvedType = Type.GetInstanceType(this.ClassNameToken.Value);
            ClassDefinition cd = this.ResolvedType.ResolvedClass;
            ConstructorDefinition ctor = cd.Constructor;
            if (ctor.Args.Length != this.Args.Length)
            {
                throw new ParserException(this, "The constructor for '" + cd.Name + "' takes in " + ctor.Args.Length + " argument(s) but found " + this.Args.Length + ".");
            }

            for (int i = 0; i < this.Args.Length; i++)
            {
                this.Args[i] = this.Args[i].ResolveTypes(resolver, ctor.Args[i].Type);
            }

            this.ResolvedType.Resolve(resolver);
            return this;
        }
    }
}
