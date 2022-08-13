using System.Linq;

namespace Vanilla.ParseTree
{
    internal class DotField : Expression
    {
        public Expression Root { get; set; }
        public Token DotToken { get; private set; }
        public Token FieldToken { get; private set; }
        public string FieldName { get; private set; }
        public TopLevelEntity ResolvedMember { get; private set; }

        public DotField(Expression root, Token dotToken, Token fieldToken) : base(root.FirstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldToken = fieldToken;
            this.FieldName = fieldToken.Value;
            this.ResolvedMember = null;
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);

            return this;
        }

        private SystemFunction CreateTypeMethod(SystemFunctionType func)
        {
            return new SystemFunction(this.Root.FirstToken, func, ((TypeRootedExpression)this.Root).Type, this.FieldToken);
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.Root = this.Root.ResolveTypes(resolver, null);

            if (this.Root is TypeRootedExpression)
            {
                Type rootType = ((TypeRootedExpression)this.Root).Type;
                switch (rootType.RootType + "." + this.FieldName)
                {
                    case "array.of": return CreateTypeMethod(SystemFunctionType.ARRAY_OF);
                    case "array.castFrom": return CreateTypeMethod(SystemFunctionType.ARRAY_CAST_FROM);
                    case "list.of": return CreateTypeMethod(SystemFunctionType.LIST_OF);
                    case "map.of": return CreateTypeMethod(SystemFunctionType.MAP_OF);

                    default:
                        throw new ParserException(this.DotToken, "The type '" + rootType + "' does not have a field named '" + this.FieldName + "'.");
                }
            }
            else
            {
                Type rootType = this.Root.ResolvedType;
                if (rootType.IsClass)
                {
                    TopLevelEntity member = rootType.ResolvedClass.GetMemberWithInheritance(this.FieldName);
                    if (member == null)
                    {
                        throw new ParserException(this.FieldToken, "The class " + rootType.ResolvedClass.Name + " does not have a member named " + this.FieldName);
                    }
                    this.ResolvedMember = member;
                    if (member is FunctionDefinition)
                    {
                        FunctionDefinition fd = member as FunctionDefinition;
                        this.ResolvedType = Type.GetFunctionType(fd.ReturnType, fd.Args.Select(arg => arg.Type).ToArray());
                    }
                    else if (member is Field)
                    {
                        Field f = member as Field;
                        this.ResolvedType = f.Type;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                else
                {
                    this.ResolvedType = this.GetPrimitiveFieldType(rootType, rootType.RootType + "." + this.FieldName);
                }
                this.ResolvedType.Resolve(resolver);
            }
            return this;
        }

        private static Type[] NO_ARGS = new Type[0];
        private Type GetPrimitiveFieldType(Type rootType, string id)
        {
            Type itemType = rootType.Generics.Length == 1 ? rootType.Generics[0] : null;
            switch (id)
            {
                case "array.length": return Type.INT;
                case "list.add": return Type.GetFunctionType(Type.VOID, new Type[] { itemType });
                case "list.length": return Type.INT;
                case "list.toArray": return Type.GetFunctionType(Type.GetArrayType(itemType), NO_ARGS);
                case "string.replace": return Type.GetFunctionType(Type.STRING, new Type[] { Type.STRING, Type.STRING });
                case "string.trim": return Type.GetFunctionType(Type.STRING, NO_ARGS);
                case "string.toCharacterArray": return Type.GetFunctionType(Type.GetArrayType(Type.STRING), NO_ARGS);
                default:
                    throw new ParserException(this.DotToken, "There is no field named " + id + " on type " + rootType.RootType + ".");
            }
        }
    }
}
