using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class FunctionInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Expression[] ArgList { get; private set; }
        public Token OpenParen { get; private set; }

        public FunctionInvocation(Expression root, Token openParen, IList<Expression> argList) : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenParen = openParen;
            this.ArgList = argList.ToArray();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            this.Root = this.Root.ResolveVariables(resolver, scope);
            for (int i = 0; i < this.ArgList.Length; i++)
            {
                this.ArgList[i] = this.ArgList[i].ResolveVariables(resolver, scope);
            }

            if (this.Root is TypeRootedExpression)
            {
                throw new System.NotImplementedException();
            }

            return this;
        }

        private void ResolveArgTypes(Resolver resolver)
        {
            for (int i = 0; i < this.ArgList.Length; i++)
            {
                this.ArgList[i] = this.ArgList[i].ResolveTypes(resolver);
            }
        }

        private SystemFunction GetRootAsSystemFunction(string typeSignature, Expression fieldFreeRoot, Token fieldToken)
        {
            Type rootType = fieldFreeRoot.ResolvedType;
            Token firstToken = fieldFreeRoot.FirstToken;
            Type g1 = rootType.Generics.Length > 0 ? rootType.Generics[0] : null;

            switch (typeSignature)
            {
                case "list.add":
                    return new SystemFunction(firstToken, SystemFunctionType.LIST_ADD, Type.GetListType(rootType.Generics[0]), fieldToken)
                    {
                        ResolvedType = Type.GetFunctionType(Type.VOID, new Type[] { g1 }),
                    };
                default:
                    throw new System.NotImplementedException();
            }
        }

        public override Expression ResolveTypes(Resolver resolver)
        {
            this.ResolveArgTypes(resolver);
            this.Root.ResolveTypes(resolver);

            if (this.Root is DotField)
            {
                DotField df = (DotField)this.Root;
                string fieldName = df.FieldName;
                df.Root = df.Root.ResolveTypes(resolver);
                Type rootType = df.Root.ResolvedType;
                string signature;
                SystemFunctionInvocation sfi = null;
                Type resolvedType;
                if (df.Root is TypeRootedExpression)
                {
                    rootType = ((TypeRootedExpression)df.Root).Type;
                }
                signature = rootType.RootType + "." + fieldName;

                switch (signature)
                {
                    case "array.castFrom":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.ARRAY_CAST_FROM, this.ArgList);
                        resolvedType = rootType;
                        break;

                    case "list.of":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.LIST_OF, this.ArgList);
                        resolvedType = rootType;
                        break;

                    case "map.of":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.MAP_OF, this.ArgList);
                        resolvedType = rootType;
                        break;

                    case "list.add":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.LIST_ADD, df.Root, this.ArgList);
                        resolvedType = Type.VOID;
                        break;

                    case "list.toArray":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.LIST_TO_ARRAY, df.Root, this.ArgList);
                        resolvedType = Type.GetArrayType(rootType.Generics[0]);
                        break;

                    default:
                        if (rootType.IsClass)
                        {
                            TopLevelEntity member = rootType.ResolvedClass.GetMemberWithInheritance(fieldName);
                            FunctionDefinition method =  member as FunctionDefinition;
                            if (method == null) throw new ParserException(this.OpenParen, "This is not a function or method and cannot be invoked as such");
                            resolvedType = method.ReturnType;
                            this.EnsureArgsCompatible(method);
                            break;
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                }

                if (sfi != null) {
                    sfi.ResolvedType = resolvedType;
                    sfi.EnsureArgsCompatible();
                    return sfi;
                }

                return this;
            }
            else if (this.Root is SystemFunction)
            {
                string name = ((SystemFunction)this.Root).Name;
                SystemFunctionInvocation sfi;
                switch (name)
                {
                    case "sqrt":
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.SQRT, this.ArgList);
                        sfi.ResolvedType = Type.FLOAT;
                        break;

                    case "floor":
                        if (this.ArgList[0].ResolvedType.IsInteger) throw new ParserException(this, "$floor was invoked on an integer type. There is need to do this.");
                        sfi = new SystemFunctionInvocation(this.FirstToken, SystemFunctionType.FLOOR, this.ArgList);
                        sfi.ResolvedType = Type.INT;
                        break;

                    default:
                        throw new ParserException(this, "Unknown system function: $" + name);
                }
                sfi.EnsureArgsCompatible();
                return sfi;
            }
            else if (this.Root is FunctionReference)
            {
                this.Root = this.Root.ResolveTypes(resolver);
                return new LocalFunctionInvocation((FunctionReference)this.Root, this.ArgList);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        private void EnsureArgsCompatible(FunctionDefinition funcDef)
        {
            if (funcDef.Args.Length != this.ArgList.Length)
            {
                throw new ParserException(this.OpenParen, "Incorrect argument count. Expected " + funcDef.Args.Length + " but found " + this.ArgList.Length + " instead.");
            }
            int argc = this.ArgList.Length;
            for (int i = 0; i < argc; i++)
            {
                Type expectedType = funcDef.Args[i].Type;
                Type actualType = this.ArgList[i].ResolvedType;
                if (expectedType.AssignableFrom(actualType))
                {
                    if (expectedType.IsFloat && actualType.IsInteger)
                    {
                        throw new ParserException(this.ArgList[i].FirstToken, "TODO: casting int to float");
                    }
                }
                else
                {
                    throw new ParserException(this.ArgList[i].FirstToken, "Incompatible argument types. Cannot assign a " + actualType + " to a " + expectedType + "."); // TODO: implement the ToString.
                }
            }
        }

        private Type ResolveSystemFunctionType(SystemFunction sysFunc, Type[] argTypes)
        {
            switch (sysFunc.SystemId)
            {
                case SystemFunctionType.SQRT: return Type.GetFunctionType(Type.FLOAT, new Type[] { Type.FLOAT });
                case SystemFunctionType.FLOOR: return Type.GetFunctionType(Type.INT, new Type[] { Type.FLOAT });
                default: break;
            }
            Type rootType = sysFunc.TypeForMethods;
            Type[] generics = rootType.Generics;
            Type keyType = generics.Length == 2 ? generics[0] : null;
            Type valueType = generics.Length == 2 ? generics[1] : null;
            Type itemType = generics.Length == 1 ? generics[0] : null;

            Type arg1Type = this.ArgList.Length > 0 ? this.ArgList[0].ResolvedType : null;

            List<Type> expectedArgTypes = new List<Type>();
            int argCount = argTypes.Length;
            Type returnType;

            switch (sysFunc.Name)
            {
                case "map.of":
                    returnType = rootType;
                    for (int i = 0; i < argCount; i += 2)
                    {
                        expectedArgTypes.Add(keyType);
                        expectedArgTypes.Add(valueType);
                    }
                    return Type.GetFunctionType(returnType, expectedArgTypes);

                case "array.castFrom":
                    this.EnsureArgCount(1);
                    if (arg1Type.RootType != "array" && arg1Type.RootType != "list") throw new ParserException(this.ArgList[0].FirstToken, "First argument of castFrom must be either a list or array");
                    if (!itemType.AssignableFrom(arg1Type.Generics[0])) throw new ParserException(this.ArgList[0].FirstToken, "Cannot convert between the items of this collection to the target type.");
                    expectedArgTypes.Add(arg1Type);
                    return Type.GetFunctionType(rootType, expectedArgTypes);

                case "array.of":
                case "list.of":
                    returnType = rootType;
                    for (int i = 0; i < argCount; i++)
                    {
                        expectedArgTypes.Add(itemType);
                    }
                    return Type.GetFunctionType(returnType, expectedArgTypes);

                default: throw new System.NotImplementedException();
            }
        }

        private void EnsureArgCount(int argc)
        {
            if (this.ArgList.Length != argc)
            {
                throw new ParserException(this.OpenParen, "Incorrect number of args. Expected " + argc + " but found " + this.ArgList.Length + ".");
            }
        }
    }
}
