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

        private void ResolveArgTypes(Resolver resolver, Type[] expectedArgTypes)
        {
            ParserException.EnsureArgCount(this, expectedArgTypes.Length, this.ArgList.Length);

            for (int i = 0; i < this.ArgList.Length; i++)
            {
                this.ArgList[i] = this.ArgList[i].ResolveTypes(resolver, expectedArgTypes[i]);
            }
        }

        private Type[] CreateTypeArrayOfRepeatedType(Type t, int times)
        {
            Type[] arr = new Type[times];
            for (int i = 0; i < times; i++)
            {
                arr[i] = t;
            }
            return arr;
        }

        private SystemFunctionInvocation ResolveTypesForSystemFunction(Resolver resolver)
        {
            SystemFunction sysFunc = (SystemFunction)this.Root;
            this.ArgList = sysFunc.RootContext == null
                ? this.ArgList.ToArray()
                : (new Expression[] { sysFunc.RootContext }).Concat(this.ArgList).ToArray();
            int argc = this.ArgList.Length;

            Type itemType = (sysFunc.RootContext == null ? null : sysFunc.RootContext.ResolvedType.ItemType)
                ?? sysFunc.FunctionReturnType.ItemType;
            Type keyType = itemType;
            Type valueType = (sysFunc.RootContext == null ? null : sysFunc.RootContext.ResolvedType.ValueType)
                ?? sysFunc.FunctionReturnType.ValueType;

            switch (sysFunc.SystemId)
            {
                case SystemFunctionType.ARRAY_CAST_FROM:
                    {
                        Type[] expectedArgs = new Type[] { Type.GetArrayType(itemType) };
                        if (argc != 1) EnsureArgsCompatible(resolver, expectedArgs, true); // intentionally throw
                        this.ArgList[0] = this.ArgList[0].ResolveTypes(resolver, null);
                        Type argType = this.ArgList[0].ResolvedType;
                        if (!argType.IsArray && !argType.IsList) EnsureArgsCompatible(resolver, expectedArgs, true); // intentionally throw
                    }
                    break;

                case SystemFunctionType.LIST_ADD:
                    EnsureArgsCompatible(resolver, new Type[] { Type.GetListType(itemType), itemType }, true);
                    break;

                case SystemFunctionType.LIST_OF:
                    EnsureArgsCompatible(resolver, CreateTypeArrayOfRepeatedType(sysFunc.FunctionReturnType.ItemType, this.ArgList.Length), false);
                    break;

                case SystemFunctionType.MAP_OF:
                    if (this.ArgList.Length % 2 != 0) throw new ParserException(this, "map<K, V>.of() requires an even number of arguments.");
                    List<Type> expectedTypes = new List<Type>();
                    for (int i = 0; i < this.ArgList.Length; i += 2)
                    {
                        expectedTypes.Add(keyType);
                        expectedTypes.Add(valueType);
                    }
                    EnsureArgsCompatible(resolver, expectedTypes, false);
                    break;

                case SystemFunctionType.STRING_REPLACE:
                    EnsureArgsCompatible(resolver, new Type[] { Type.STRING, Type.STRING, Type.STRING }, true);
                    break;

                case SystemFunctionType.STRING_TO_CHARACTER_ARRAY:
                    EnsureArgsCompatible(resolver, new Type[] { Type.STRING }, true);
                    break;

                case SystemFunctionType.STRING_TRIM:
                    EnsureArgsCompatible(resolver, new Type[] { Type.STRING }, true);
                    break;

                default:
                    throw new System.NotImplementedException(sysFunc.SystemId.ToString());
            }

            return new SystemFunctionInvocation(this.FirstToken, sysFunc.SystemId, this.ArgList)
            {
                ResolvedType = sysFunc.FunctionReturnType,
            };
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            this.Root = this.Root is DotField
                ? ((DotField)this.Root).ResolveTypesAsFunctionInvocationTarget(resolver)
                : this.Root.ResolveTypes(resolver, nullHint);

            if (this.Root is SystemFunction)
            {
                return this.ResolveTypesForSystemFunction(resolver);
            }

            if (this.Root.ResolvedType == null) throw new ParserException(this.OpenParen, "This type of expression cannot be invoked like a function.");

            FunctionReference funcRef = this.Root as FunctionReference;
            if (funcRef != null)
            {
                this.EnsureArgsCompatible(resolver, funcRef.FunctionDefinition.Args, funcRef.InstanceContext != null);
                if (funcRef.InstanceContext == null)
                {
                    return new LocalFunctionInvocation(funcRef, this.ArgList);
                }
                return new MethodInvocation(funcRef.InstanceContext, funcRef, this.ArgList);
            }

            throw new System.NotImplementedException();
        }

        private void EnsureArgsCompatible(Resolver resolver, IList<VariableDeclaration> expected, bool usesRootContext)
        {
            EnsureArgsCompatible(resolver, expected.Select(vd => vd.Type).ToArray(), usesRootContext);
        }

        private void EnsureArgsCompatible(Resolver resolver, IList<Type> expected, bool usesRootContext)
        {
            int argc = this.ArgList.Length;
            ParserException.EnsureArgCount(this, expected.Count, argc);

            for (int i = 0; i < argc; i++)
            {
                this.ArgList[i] = this.EnsureArgCompatible(resolver, expected[i], this.ArgList[i], i == 0 && usesRootContext);
            }
        }

        private Expression EnsureArgCompatible(Resolver resolver, Type expectedType, Expression arg, bool isRootContext)
        {
            if (!isRootContext)
            {
                arg = arg.ResolveTypes(resolver, expectedType);
            }

            Type actualType = arg.ResolvedType;

            if (!expectedType.AssignableFrom(actualType))
            {
                throw new ParserException(arg.FirstToken, "Incompatible argument types. Cannot assign a " + actualType + " to a " + expectedType + ".");
            }

            if (expectedType.IsFloat && actualType.IsInteger)
            {
                throw new ParserException(arg.FirstToken, "TODO: casting int to float");
            }
            return arg;
        }
    }
}
