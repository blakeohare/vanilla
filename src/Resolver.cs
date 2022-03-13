using System.Collections.Generic;
using System.Linq;
using Vanilla.ParseTree;

namespace Vanilla
{
    internal class Resolver
    {
        private TopLevelEntity[] entities;

        private Dictionary<string, TopLevelEntity> entityByName;
        private FunctionDefinition[] allFunctions;
        private Field[] topLevelFields;
        private ClassDefinition[] allClasses;
        private EnumDefinition[] allEnums;

        public Resolver(IList<TopLevelEntity> entities)
        {
            this.entities = entities.ToArray();
            this.allFunctions = entities.OfType<FunctionDefinition>().ToArray();
            this.topLevelFields = entities.OfType<Field>().ToArray();
            this.allClasses = entities.OfType<ClassDefinition>().ToArray();
            this.allEnums = entities.OfType<EnumDefinition>().ToArray();
        }

        internal FunctionDefinition GetFunctionByName(string name)
        {
            if (this.entityByName.ContainsKey(name))
            {
                return this.entityByName[name] as FunctionDefinition;
            }
            return null;
        }

        internal ClassDefinition GetClassByName(string name)
        {
            if (this.entityByName.ContainsKey(name))
            {
                return this.entityByName[name] as ClassDefinition;
            }
            return null;
        }

        public void Resolve()
        {
            this.CreateEntityNameMapping();
            this.ResolveVariableDeclarations();
            this.ResolveSignatureTypes();
            this.ResolveTypes();
        }

        private void CreateEntityNameMapping()
        {
            this.entityByName = new Dictionary<string, TopLevelEntity>();
            foreach (TopLevelEntity tle in entities)
            {
                string name;
                if (tle is ClassDefinition)
                {
                    name = ((ClassDefinition)tle).Name;
                }
                else if (tle is FunctionDefinition)
                {
                    name = ((FunctionDefinition)tle).Name;
                }
                else if (tle is EnumDefinition)
                {
                    name = ((EnumDefinition)tle).NameToken.Value;
                }
                else if (tle is Field)
                {
                    Field f = (Field)tle;
                    if (!f.IsStatic) throw new ParserException(f.FirstToken, "Only static fields can exist outside of classes.");
                    name = f.Name;
                }
                else
                {
                    throw new ParserException(tle.FirstToken, "This entity is not valid at the root level.");
                }
                this.entityByName[name] = tle;
            }
        }

        private void ResolveVariableDeclarations()
        {
            List<FunctionDefinition> functions = new List<FunctionDefinition>(this.allFunctions);
            foreach (ClassDefinition cd in this.allClasses)
            {
                functions.AddRange(cd.Members.OfType<FunctionDefinition>());
            }

            foreach (FunctionDefinition fd in functions)
            {
                fd.ResolveVariables(this);
            }
        }

        private IList<FunctionDefinition> AllFunctionsIncludingNested()
        {
            List<FunctionDefinition> functions = new List<FunctionDefinition>(this.allFunctions);
            foreach (ClassDefinition cd in this.allClasses)
            {
                functions.AddRange(cd.Members.OfType<FunctionDefinition>());
            }
            return functions;
        }

        private void ResolveSignatureTypes()
        {
            foreach (FunctionDefinition fd in AllFunctionsIncludingNested())
            {
                fd.ResolveSignatureTypes(this);
            }
        }

        private void ResolveTypes()
        {
            foreach (FunctionDefinition fd in AllFunctionsIncludingNested())
            {
                fd.ResolveTypes(this);
            }
        }
    }
}
