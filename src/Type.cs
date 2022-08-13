using System.Collections.Generic;
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
        public bool IsResolved { get; set; }

        private static readonly Type[] EMPTY_GENERICS = new Type[0];
        public static readonly Type INT = new Type() { IsResolved = true, RootType = "int" };
        public static readonly Type BOOL = new Type() { IsResolved = true, RootType = "bool" };
        public static readonly Type FLOAT = new Type() { IsResolved = true, RootType = "float" };
        public static readonly Type STRING = new Type() { IsResolved = true, RootType = "string" };
        public static readonly Type TYPE = new Type() { IsResolved = true, RootType = "type" };
        public static readonly Type VOID = new Type() { IsResolved = true, RootType = "void" };

        public bool IsArray { get { return this.RootType == "array"; } }
        public bool IsList { get { return this.RootType == "list"; } }
        public bool IsNumeric { get { return this.RootType == "int" || this.RootType == "float"; } }
        public bool IsInteger { get { return this.RootType == "int"; } }
        public bool IsFloat { get { return this.RootType == "float"; } }
        public bool IsString { get { return this.RootType == "string"; } }
        public bool IsBoolean { get { return this.RootType == "bool"; } }

        public Type ItemType { get { return this.Generics.Length > 0 ? this.Generics[0] : null; } }
        public Type KeyType { get { return this.ItemType; } }
        public Type ValueType { get { return this.Generics.Length > 1 ? this.Generics[1] : null; } }

        public Type()
        {
            this.Generics = EMPTY_GENERICS;
            this.IsClass = false;
            this.ResolvedClass = null;
            this.IsResolved = false;
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

        public static Type GetFunctionType(Type returnType, IList<Type> argTypes)
        {
            List<Type> generics = new List<Type>() { returnType };
            generics.AddRange(argTypes);
            return new Type() { FirstToken = null, Generics = generics.ToArray(), RootType = "func" };
        }

        public static Type GetListType(Type itemType)
        {
            return new Type() { FirstToken = null, Generics = new Type[] { itemType }, RootType = "list" };
        }

        public static Type GetArrayType(Type itemType)
        {
            return new Type() { FirstToken = null, Generics = new Type[] { itemType }, RootType = "array" };
        }

        public static Type GetMapType(Type keyType, Type valueType)
        {
            return new Type() { FirstToken = null, Generics = new Type[] { keyType, valueType }, RootType = "map" };
        }

        public static Type GetInstanceType(string className)
        {
            return new Type() { FirstToken = null, RootType = className, IsClass = true };
        }

        public void Resolve(Resolver resolver)
        {
            if (this.IsResolved) return;
            if (this.IsClass)
            {
                ClassDefinition cd = resolver.GetClassByName(this.RootType);
                if (cd == null) throw new ParserException(this.FirstToken, "The type '" + this.RootType + "' could not be resolved.");
                this.ResolvedClass = cd;
            }
            foreach (Type gen in this.Generics)
            {
                gen.Resolve(resolver);
            }
            this.IsResolved = true;
        }

        public bool AssignableFrom(Type otherType)
        {
            if (this.RootType == "void") return false;
            if (otherType.RootType == "void") return false;
            if (otherType == this) return true;
            if (this.RootType == "object") return true;
            if (otherType.RootType == "int" && this.RootType == "float") return true;
            if (this.Generics.Length != otherType.Generics.Length) return false;
            if (this.Generics.Length == 0)
            {
                return this.RootType == otherType.RootType;
            }
            if (this.IsClass != otherType.IsClass) return false;
            if (this.IsClass)
            {
                throw new System.NotImplementedException();
            }
            if (otherType.RootType != this.RootType) return false;
            throw new System.NotImplementedException();
        }
    }
}
