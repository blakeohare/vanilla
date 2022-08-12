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
            this.EntParser = new EntityParser(this);
            this.ExecParser = new ExecutableParser(this);
            this.ExprParser = new ExpressionParser(this);
            this.TypeParser = new TypeParser();

            List<TopLevelEntity> entities = new List<TopLevelEntity>();
            Stack<TokenStream> tokenStack = new Stack<TokenStream>();
            HashSet<string> filesParsedCanonicalPath = new HashSet<string>();

            this.ParseFile(startingFile, entities, tokenStack, filesParsedCanonicalPath);

            Resolver resolver = new Resolver(entities);
            resolver.Resolve();

            return new ParseBundle()
            {
                ClassDefinitions = entities.OfType<ClassDefinition>().ToArray(),
                FunctionDefinitions = entities.OfType<FunctionDefinition>().ToArray(),
                EnumDefinitions = entities.OfType<EnumDefinition>().ToArray(),
                FieldDefinitions = entities.OfType<Field>().ToArray(),
                StringDefinitions = resolver.GetAllStringConstantsById().ToArray(),
            };
        }

        private void ParseFile(
            string file,
            List<TopLevelEntity> entitiesOut,
            Stack<TokenStream> tokenStack,
            HashSet<string> filesParsedCanonicalPath)
        {
            if (filesParsedCanonicalPath.Contains(file)) return;
            filesParsedCanonicalPath.Add(file);

            TokenStream tokens = Tokenizer.Tokenize(file);
            this.UpdateParseTokenRefs(tokens);
            tokenStack.Push(tokens);

            List<ImportStatement> imports = new List<ImportStatement>();
            while (tokens.HasMore && tokens.PeekValue() == "import")
            {
                Token importToken = tokens.PopExpected("import");
                tokens.EnsureNotEof();
                StringConstant pathStringExpr = this.ExprParser.ParseExpression(null) as StringConstant;
                if (pathStringExpr == null) throw new ParserException(importToken, "import expression must be a string constant.");
                tokens.PopExpected(";");
                ImportStatement import = new ImportStatement(importToken, pathStringExpr.Value);
                imports.Add(import);
                string filePath = System.IO.Path.Combine(file, "..", import.Path);
                string absPathComplex = filePath.Replace('/', System.IO.Path.DirectorySeparatorChar).Replace('\\', System.IO.Path.DirectorySeparatorChar);
                string absPathFlat = System.IO.Path.GetFullPath(absPathComplex);
                if (!System.IO.File.Exists(absPathFlat))
                {
                    throw new ParserException(pathStringExpr, "Import path does not exist: " +  absPathFlat);
                }
                this.ParseFile(absPathFlat, entitiesOut, tokenStack, filesParsedCanonicalPath);
            }

            while (tokens.HasMore)
            {
                TopLevelEntity ent = this.EntParser.ParseEntity();
                if (ent != null)
                {
                    entitiesOut.Add(ent);
                }
            }
            tokenStack.Pop();
            this.UpdateParseTokenRefs(tokenStack.Count > 0 ? tokenStack.Peek() : null);
        }

        private void UpdateParseTokenRefs(TokenStream tokens)
        {
            this.EntParser.SetTokens(tokens);
            this.ExecParser.SetTokens(tokens);
            this.ExprParser.SetTokens(tokens);
            this.TypeParser.SetTokens(tokens);
        }
    }
}
