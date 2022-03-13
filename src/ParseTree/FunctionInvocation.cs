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
                this.ArgList[i].ResolveTypes(resolver);
            }
        }

        public override void ResolveTypes(Resolver resolver)
        {
            SystemFunction sf = this.Root as SystemFunction;
            if (sf != null)
            {
                this.ResolveArgTypes(resolver);
                this.Root.ResolvedType = this.ResolveSystemFunctionType(sf, this.ArgList.Select(expr => expr.ResolvedType).ToArray());
                this.Root.ResolvedType.Resolve(resolver);
            }
            else
            {
                this.Root.ResolveTypes(resolver);
                this.ResolveArgTypes(resolver);
            }

            Type funcType = this.Root.ResolvedType;
            if (funcType.RootType != "func") throw new ParserException(this.Root.FirstToken, "This expression cannot be invoked like a function.");
            int expectedArgCount = funcType.Generics.Length - 1;
            int actualArgCount = this.ArgList.Length;
            if (expectedArgCount != actualArgCount) throw new ParserException(this.OpenParen, "This function has the wrong number of arguments. Expected " + expectedArgCount + " but found " + actualArgCount + ".");

            for (int i = 0; i < this.ArgList.Length; i++)
            {
                Type actualType = this.ArgList[i].ResolvedType;
                Type expectedType = funcType.Generics[i + 1];
                if (!expectedType.AssignableFrom(actualType))
                {
                    throw new ParserException(
                        this.ArgList[i].FirstToken,
                        "Incorrect argument type. Expected '" + expectedType.ToString() + "' but received '" + actualType.ToString() + "' in argument " + (i + 1));
                }
            }

            this.ResolvedType = funcType.Generics[0];
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
