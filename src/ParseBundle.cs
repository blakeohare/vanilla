using Vanilla.ParseTree;

namespace Vanilla
{
    internal class ParseBundle
    {
        public ClassDefinition[] ClassDefinitions { get; set; }
        public FunctionDefinition[] FunctionDefinitions { get; set; }
        public EnumDefinition[] EnumDefinitions { get; set; }
        public Field[] FieldDefinitions { get; set; }
        public string[] StringDefinitions { get; set; }
    }
}
