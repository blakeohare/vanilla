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

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            Expression result = this.TryResolveTypesImpl(resolver, null);
            if (result.ResolvedType == null)
            {
                throw new ParserException(this, "This expression must be invoked as a function.");
            }
            return result;
        }

        internal Expression ResolveTypesAsFunctionInvocationTarget(Resolver resolver)
        {
            return this.TryResolveTypesImpl(resolver, null);
        }

        /*
            This should catch all systes function invocations.
            
            TYPE-ROOTED EXPRESSIONS: array<int>.castFrom(myList)
                This get converted into a system function with a ResolvedType == null.
                This is because it needs the arguments to know what to resolve the type to.
                This is ONLY allowed to happen when called from the FunctionInvocation's ResolveTypes
                method using a special if statement. If this returns a value that has 
                a .ResolvedType == null when called by the DotField.ResolveTypes method then this 
                will throw a ParserException. See ResolveTypesAsFunctionInvocationTarget.
            
            PRIMITIVE INSTANCE METHODS: myString.replace(a, b)
                This will get converted into a SystemFunction as well, but will additionally
                have the RootContext set to a non-null value.
            
            STANDALONE PRIMITIVE PROPERTIES: myArray.length
                This will get converted into a SystemFunctionINVOCATION. In other words, the parser
                after this point will treat it as if it were a array.length(). It will have a root
                context value of the root value and its ResolvedType will not be null.
        */

        private Expression TryResolveTypesImpl(Resolver resolver, Type nullHint)
        {
            this.Root = this.Root.ResolveTypes(resolver, null);

            Token firstToken = this.FirstToken;

            if (this.Root is TypeRootedExpression)
            {
                TypeRootedExpression tre = (TypeRootedExpression)this.Root;

                SystemFunctionType sysFuncType = SystemFunctionType.UNKNOWN;
                string signature = tre.Type.RootType + "." + this.FieldName;
                switch (signature)
                {
                    case "array.castFrom": sysFuncType = SystemFunctionType.ARRAY_CAST_FROM; break;
                    case "array.of": sysFuncType = SystemFunctionType.ARRAY_OF; break;
                    case "list.of": sysFuncType = SystemFunctionType.LIST_OF; break;
                    case "map.of": sysFuncType = SystemFunctionType.MAP_OF; break;
                }

                if (sysFuncType == SystemFunctionType.UNKNOWN)
                {
                    throw new ParserException(this.DotToken, "The type '" + tre.Type.RootType + "' does not have a field named '" + this.FieldName + "'.");
                }
                return new SystemFunction(firstToken, sysFuncType, tre.Type, this.FieldToken);
            }

            Type rootType = this.Root.ResolvedType;
            if (rootType.IsClass)
            {
                TopLevelEntity member = rootType.ResolvedClass.GetMemberWithInheritance(this.FieldName);
                if (member == null) throw new ParserException(this.FieldToken, "The class " + rootType.ResolvedClass.Name + " does not have a member named " + this.FieldName);

                this.ResolvedMember = member;
                if (member is FunctionDefinition)
                {
                    FunctionDefinition funcDef = (FunctionDefinition)member;
                    FunctionReference fr = new FunctionReference(
                        this.FirstToken,
                        funcDef,
                        Type.GetFunctionType(funcDef.ReturnType, funcDef.Args.Select(arg => arg.Type).ToArray()));
                    fr.InstanceContext = this.Root;
                    return fr;
                }

                if (member is Field)
                {
                    return new FieldReference(this.FirstToken, (Field)member, this.Root);
                }

                throw new System.NotImplementedException();
            }

            SystemFunctionType sysFunc = SystemFunctionType.UNKNOWN;
            Type funcReturnType = Type.VOID;
            bool useDummyInvocation = false;
            switch (rootType.RootType + "." + this.FieldName)
            {
                case "array.length":
                    sysFunc = SystemFunctionType.ARRAY_LENGTH;
                    funcReturnType = Type.INT;
                    useDummyInvocation = true;
                    break;
                case "list.add":
                    sysFunc = SystemFunctionType.LIST_ADD;
                    funcReturnType = Type.VOID;
                    break;
                case "list.length":
                    sysFunc = SystemFunctionType.LIST_LENGTH;
                    funcReturnType = Type.INT;
                    useDummyInvocation = true;
                    break;
                case "map.contains":
                    sysFunc = SystemFunctionType.MAP_CONTAINS;
                    funcReturnType = Type.BOOL;
                    break;
                case "list.toArray":
                    sysFunc = SystemFunctionType.LIST_TO_ARRAY;
                    funcReturnType = Type.GetArrayType(rootType.ItemType);
                    break;
                case "string.firstCharCode":
                    sysFunc = SystemFunctionType.STRING_FIRST_CHAR_CODE;
                    funcReturnType = Type.INT;
                    break;
                case "string.replace":
                    sysFunc = SystemFunctionType.STRING_REPLACE;
                    funcReturnType = Type.STRING;
                    break;
                case "string.toCharacterArray":
                    sysFunc = SystemFunctionType.STRING_TO_CHARACTER_ARRAY;
                    funcReturnType = Type.GetArrayType(Type.STRING);
                    break;
                case "string.trim":
                    sysFunc = SystemFunctionType.STRING_TRIM;
                    funcReturnType = Type.STRING;
                    break;
            }

            if (sysFunc == SystemFunctionType.UNKNOWN)
            {
                throw new ParserException(this.DotToken, "The type '" + rootType.RootType + "' does not have a field named + '." + this.FieldName + "'.");
            }

            SystemFunction sysFuncInstance = new SystemFunction(this.FirstToken, sysFunc, funcReturnType, this.FieldToken);
            sysFuncInstance.RootContext = this.Root;
            if (useDummyInvocation)
            {
                return new SystemFunctionInvocation(this.FirstToken, sysFunc, this.Root, new Expression[0]) { ResolvedType = funcReturnType };
            }
            return sysFuncInstance;
        }
    }
}
