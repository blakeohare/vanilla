using System.Collections.Generic;

namespace Vanilla.ParseTree
{
    internal class Modifiers
    {
        private Dictionary<string, Token> modifiers = new Dictionary<string, Token>();

        public Token FirstToken { get; private set; }

        public bool IsPublic { get { return this.modifiers.ContainsKey("public"); } }

        public Modifiers(Token firstToken, ICollection<Token> modifierNames)
        {
            this.FirstToken = firstToken;
            foreach (Token modifier in modifierNames)
            {
                string name = modifier.Value;
                switch (name)
                {
                    case "public":
                        break;
                    default:
                        throw new ParserException(modifier, "'" + name + "' is not a valid modifier.");
                }

                if (modifiers.ContainsKey(name)) throw new ParserException(modifier, "The modifier '" + name + "' appears multiple times");
                modifiers[name] = modifier;
            }
        }
    }
}
