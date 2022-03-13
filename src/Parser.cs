using System.Collections.Generic;
using System.Linq;
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

        public ParseBundle Parse(string startingFile)
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

            return new ParseBundle()
            {
                ClassDefinitions = entities.OfType<ClassDefinition>().ToArray(),
                FunctionDefinitions = entities.OfType<FunctionDefinition>().ToArray(),
                EnumDefinitions = entities.OfType<EnumDefinition>().ToArray(),
                FieldDefinitions = entities.OfType<Field>().ToArray(),
            };
        }
    }
}
