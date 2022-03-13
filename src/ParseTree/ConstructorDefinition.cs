using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class ConstructorDefinition : TopLevelEntity
    {
        public Token[] ArgDeclarations { get; private set; }
        public Type[] ArgTypes { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Token BaseToken { get; private set; }
        public Expression[] BaseArgs { get; private set; }
        public Executable[] Body { get; set; }

        public ConstructorDefinition(Token constructorToken, IList<Token> argDeclarations, IList<Type> argTypes, IList<Token> argNames, Token baseToken, IList<Expression> baseArgs)
            : base(constructorToken)
        {
            this.ArgDeclarations = argDeclarations.ToArray();
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.BaseToken = baseToken;
            this.BaseArgs = baseArgs.ToArray();
        }
    }
}
