using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla.ParseTree
{
    internal class SystemFunctionInvocation : Expression
    {
        public SystemFunctionType SysFuncId { get; private set; }
        public Expression[] ArgList { get; private set; }

        private bool usesFakeFirstArg = false;

        public SystemFunctionInvocation(Token firstToken, SystemFunctionType sysFuncId, IList<Expression> args) : base(firstToken)
        {
            this.SysFuncId = sysFuncId;
            this.ArgList = args.ToArray();
            foreach (Expression arg in this.ArgList)
            {
                if (arg.ResolvedType == null) throw new Exception(); // should not happen!
            }
        }

        public SystemFunctionInvocation(Token firstToken, SystemFunctionType sysFuncId, Expression rootExpr, IList<Expression> args) :
            this(firstToken, sysFuncId, new Expression[] { rootExpr }.Concat(args).ToArray())
        {
            this.usesFakeFirstArg = true;
        }

        public override Expression ResolveTypes(Resolver resolver, Type nullHint)
        {
            // Generated as a result of the type resolver
            throw new NotImplementedException();
        }

        public override Expression ResolveVariables(Resolver resolver, LexicalScope scope)
        {
            // Generated after this phase.
            throw new NotImplementedException();
        }

        internal void EnsureArgsCompatible()
        {
            Expression arg1 = this.ArgList.Length > 0 ? this.ArgList[0] : null;
            Expression arg2 = this.ArgList.Length > 1 ? this.ArgList[1] : null;
            Expression arg3 = this.ArgList.Length > 2 ? this.ArgList[2] : null;

            switch (this.SysFuncId)
            {
                case SystemFunctionType.ARRAY_CAST_FROM:
                    {
                        this.EnsureArgCount(1);
                        this.EnsureIsCollection(arg1.FirstToken, arg1.ResolvedType, "First argument of castFrom must be either a list or array");
                        Type expectedItemType = this.ResolvedType.Generics[0];
                        Type actualItemType = arg1.ResolvedType.Generics[0];
                        if (!expectedItemType.AssignableFrom(actualItemType)) throw new ParserException(arg1, "Cannot convert between the items of this collection to the target type.");
                    }
                    break;

                case SystemFunctionType.LIST_ADD:
                    {
                        this.EnsureArgCount(2);
                        Type itemType = this.ArgList[0].ResolvedType.Generics[0];
                        if (!itemType.AssignableFrom(this.ArgList[1].ResolvedType)) throw new ParserException(this.ArgList[1], "Cannot add an item of this type to this list. Types are incompatible.");
                    }
                    break;

                case SystemFunctionType.LIST_OF:
                    {
                        Type itemType = this.ResolvedType.Generics[0];
                        for (int i = 0; i < this.ArgList.Length; i++)
                        {
                            if (!itemType.AssignableFrom(this.ArgList[i].ResolvedType)) throw new ParserException(this.ArgList[i], "Invalid type for argument. Expected '" + itemType + "' instead.");
                        }
                    }
                    break;

                case SystemFunctionType.LIST_TO_ARRAY:
                    {
                        this.EnsureArgCount(1);
                    }
                    break;

                case SystemFunctionType.MAP_OF:
                    {
                        Type keyType = this.ResolvedType.Generics[0];
                        Type valueType = this.ResolvedType.Generics[1];
                        if (this.ArgList.Length % 2 != 0) throw new ParserException(this, "There are an invalid number of args passed to map.of(). Args must be key value pairs, and thus there must be an even number of them.");
                        for (int i = 0; i < this.ArgList.Length; i += 2)
                        {
                            Expression keyExpr = this.ArgList[i];
                            Expression valueExpr = this.ArgList[i + 1];
                            if (!keyType.AssignableFrom(keyExpr.ResolvedType)) throw new ParserException(keyExpr, "Invalid argument type. Expected '" + keyExpr.ResolvedType);
                            if (!valueType.AssignableFrom(valueExpr.ResolvedType)) throw new ParserException(valueExpr, "Invalid argument type. Expected '" + valueExpr.ResolvedType);
                        }
                    }
                    break;

                case SystemFunctionType.SQRT:
                    {
                        this.EnsureArgCount(1);
                        if (!arg1.ResolvedType.IsNumeric) throw new ParserException(arg1, "$sqrt requires a numeric argument.");
                    }
                    break;

                case SystemFunctionType.STRING_REPLACE:
                    {
                        this.EnsureArgCount(3);
                        if (!arg2.ResolvedType.IsString) throw new ParserException(arg2, "string.replace() requires two strings as arguments.");
                        if (!arg3.ResolvedType.IsString) throw new ParserException(arg3, "string.replace() requires two strings as arguments.");
                    }
                    break;

                case SystemFunctionType.STRING_TO_CHARACTER_ARRAY:
                case SystemFunctionType.STRING_TRIM:
                    {
                        this.EnsureArgCount(1);
                    }
                    break;

                case SystemFunctionType.FLOOR:
                    throw new NotImplementedException();

                default:
                    throw new Exception();
            }
        }

        private void EnsureIsCollection(Token throwToken, Type type, string msg)
        {
            if (type.IsList || type.IsArray) return;
            throw new ParserException(throwToken, msg);
        }

        private void EnsureArgCount(int expectedCount)
        {
            int actual = this.ArgList.Length;
            if (actual != expectedCount)
            {
                if (this.usesFakeFirstArg)
                {
                    actual--;
                    expectedCount--;
                }

                throw new ParserException(this, "This function takes " + expectedCount + " argument(s) but found " + actual);
            }
        }
    }
}
