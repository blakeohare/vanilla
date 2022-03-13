using System.Collections.Generic;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class Parser
    {
        public EntityParser EntParser { get; private set; }
        public ExecutableParser ExecParser { get; private set; }
        public ExpressionParser ExprParser { get; private set; }
        public TypeParser TypeParser { get; private set; }

        public Parser() { }

        public void Parse(string startingFile)
        {
            TokenStream tokens = Tokenizer.Tokenize(startingFile);

            this.EntParser = new EntityParser(this, tokens);
            this.ExecParser = new ExecutableParser(this, tokens);
            this.ExprParser = new ExpressionParser(this, tokens);
            this.TypeParser = new TypeParser(tokens);

            List<TopLevelEntity> entities = new List<TopLevelEntity>();

            while (tokens.HasMore)
            {
                TopLevelEntity ent = this.EntParser.ParseEntity();
                if (ent != null)
                {
                    entities.Add(ent);
                }
            }

            Resolver resolver = new Resolver(entities);
            resolver.Resolve();
        }
    }
}
