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
        
        private List<string> allStringConstants;
        private Dictionary<string, int> stringConstantToEntryIndex;

        public Resolver(IList<TopLevelEntity> entities)
        {
            this.entities = entities.ToArray();
            this.allFunctions = entities.OfType<FunctionDefinition>().ToArray();
            this.topLevelFields = entities.OfType<Field>().ToArray();
            this.allClasses = entities.OfType<ClassDefinition>().ToArray();
            this.allEnums = entities.OfType<EnumDefinition>().ToArray();
        }

        public IList<string> GetAllStringConstantsById()
        {
            return this.allStringConstants.ToArray();
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
                cd.Constructor.ResolveVariables(this);
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
            foreach (ClassDefinition cd in this.allClasses)
            {
                cd.Constructor.ResolveArgTypes(this);
            }
        }

        private void ResolveTypes()
        {
            this.allStringConstants = new List<string>();
            this.stringConstantToEntryIndex = new Dictionary<string, int>();
            foreach (FunctionDefinition fd in AllFunctionsIncludingNested())
            {
                fd.ResolveTypes(this);
            }

            foreach (ClassDefinition cd in this.allClasses)
            {
                cd.Constructor.ResolveTypes(this);
            }
        }

        internal int RegisterStringConstant(string value)
        {
            if (this.stringConstantToEntryIndex.ContainsKey(value))
            {
                return this.stringConstantToEntryIndex[value];
            }
            int index = this.allStringConstants.Count;
            this.stringConstantToEntryIndex[value] = index;
            this.allStringConstants.Add(value);
            return index;
        }
    }
}
