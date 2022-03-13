using Vanilla.ParseTree;

namespace Vanilla
{
    internal class Type
    {
        public Token FirstToken { get; set; }
        public string RootType { get; set; }
        public bool IsClass { get; set; }
        public Type[] Generics { get; set; }
        public ClassDefinition ResolvedClass { get; set; }

        private static readonly Type[] EMPTY_GENERICS = new Type[0];

        public Type()
        {
            this.Generics = EMPTY_GENERICS;
            this.IsClass = false;
            this.ResolvedClass = null;
        }

        public override string ToString()
        {
            string value = "Type: '" + this.RootType;
            if (this.Generics.Length > 0)
            {
                value += "<";
                for (int i = 0; i < this.Generics.Length; i++)
                {
                    if (i > 0) value += ", ";
                    value += this.Generics[i].ToString();
                }
                value += ">";
            }
            return value + "'";
        }
    }
}
