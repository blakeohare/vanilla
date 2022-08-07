using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vanilla.ParseTree;

namespace Vanilla.Transpiler
{
    internal abstract class AbstractTranspiler
    {
        public bool RequiresSignatureDeclaration { get; protected set; }

        private ParseBundle bundle;

        public string NL { get; private set; }
        public string TabChar { get; private set; }

        public StringBuilder sb;

        private List<string> tabs = new List<string>();

        public int TabLevel { get; set; }

        public string CurrentTab
        {
            get
            {
                while (this.TabLevel >= this.tabs.Count)
                {
                    this.tabs.Add(this.tabs[this.tabs.Count - 1] + this.TabChar);
                }
                return this.tabs[this.TabLevel];
            }
        }

        public FunctionDefinition[] GetPublicFunctions()
        {
            return this.bundle.FunctionDefinitions
                .Where(fd => fd.IsPublic)
                .OrderBy(fd => fd.Name)
                .ToArray();
        }

        public abstract void EmitFiles(string verifiedDestinationPath);

        public AbstractTranspiler Append(string s)
        {
            this.sb.Append(s);
            return this;
        }

        public AbstractTranspiler Append(char c)
        {
            this.sb.Append(c);
            return this;
        }

        public AbstractTranspiler(ParseBundle bundle)
        {
            this.bundle = bundle;
            this.RequiresSignatureDeclaration = false;
            this.NL = "\n";
            this.TabChar = "    ";
            this.sb = new StringBuilder();
            this.tabs.Add("");
        }

        public string GenerateMainFile()
        {
            ClassDefinition[] classes = GetClassesInDependencyOrder(bundle);
            FunctionDefinition[] functions = bundle.FunctionDefinitions;

            // TODO: serialize classes

            if (this.RequiresSignatureDeclaration)
            {
                foreach (FunctionDefinition fd in functions)
                {
                    this.SerializeFunctionSignature(fd);
                }
            }

            foreach (FunctionDefinition fd in functions)
            {
                this.SerializeFunction(fd);
            }
            return this.sb.ToString();
        }

        protected abstract void SerializeArithmeticPairOp(ArithmeticPairOp apo, bool useWrap);
        protected abstract void SerializeAssignmentToVariable(Variable target, Assignment asgn, bool omitSemicolon);
        protected abstract void SerializeAssignmentToMap(MapAccess target, Assignment asgn, bool omitSemicolon);
        protected abstract void SerializeAssignmentToField(DotField target, Assignment asgn, bool omitSemicolon);
        protected abstract void SerializeBasicFunctionInvocation(Expression root, Expression[] args, bool useWrap);
        protected abstract void SerializeBooleanConstant(BooleanConstant bc, bool useWrap);
        protected abstract void SerializeExpressionAsExecutable(ExpressionAsExecutable exex, bool omitSemicolon);
        protected abstract void SerializeForLoop(ForLoop floop);
        protected abstract void SerializeForRangeLoop(ForRangeLoop frl);
        protected abstract void SerializeFunction(FunctionDefinition fd);
        protected abstract void SerializeFunctionSignature(FunctionDefinition fd);
        protected abstract void SerializeIfStatement(IfStatement ifst);
        protected abstract void SerializeIntegerConstant(IntegerConstant ic, bool useWrap);
        protected abstract void SerializeLocalFunctionInvocation(LocalFunctionInvocation lfi, bool useWrap);
        protected abstract void SerializeMapAccess(MapAccess ma, bool useWrap);
        protected abstract void SerializePairComparision(PairComparison pc, bool useWrap);
        protected abstract void SerializeReturnStatement(ReturnStatement rs);
        protected abstract void SerializeStringConstant(StringConstant sc, bool useWrap);
        protected abstract void SerializeVariable(Variable vd, bool useWrap);
        protected abstract void SerializeVariableDeclaration(VariableDeclaration vd, bool omitSemicolon);

        protected abstract void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection, bool useWrap);
        protected abstract void SerializeSysFuncFloor(Expression expr, bool useWrap);
        protected abstract void SerializeSysFuncListAdd(Type itemType, Expression listExpr, Expression itemExpr, bool useWrap);
        protected abstract void SerializeSysFuncListOf(Type itemType, Expression[] args, bool useWrap);
        protected abstract void SerializeSysFuncListToArray(Type itemType, Expression listExpr, bool useWrap);
        protected abstract void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args, bool useWrap);
        protected abstract void SerializeSysFuncSqrt(Expression expr, bool useWrap);

        internal void SerializeExecutable(Executable ex, bool omitSemicolon)
        {
            string name = ex.GetType().Name;
            switch (name)
            {
                case "Assignment":
                    // Maybe this should be in the resolver?
                    Assignment asgn = (Assignment)ex;
                    if (asgn.Target is Variable)
                    {
                        this.SerializeAssignmentToVariable((Variable)asgn.Target, asgn, omitSemicolon);
                    }
                    else if (asgn.Target is MapAccess)
                    {
                        this.SerializeAssignmentToMap((MapAccess)asgn.Target, asgn, omitSemicolon);
                    }
                    else if (asgn.Target is DotField)
                    {
                        this.SerializeAssignmentToField((DotField)asgn.Target, asgn, omitSemicolon);
                    }
                    else
                    {
                        throw new Exception(); // should not happen
                    }
                    return;
                case "ExpressionAsExecutable": this.SerializeExpressionAsExecutable((ExpressionAsExecutable)ex, omitSemicolon); break;
                case "ForLoop": this.SerializeForLoop((ForLoop)ex); break;
                case "IfStatement": this.SerializeIfStatement((IfStatement)ex); break;
                case "ReturnStatement": this.SerializeReturnStatement((ReturnStatement)ex); break;
                case "VariableDeclaration": this.SerializeVariableDeclaration((VariableDeclaration)ex, omitSemicolon); break;
                case "ForRangeLoop": this.SerializeForRangeLoop((ForRangeLoop)ex); break;
                default: throw new NotImplementedException(name);
            }
        }

        protected void SerializeExpression(Expression expr, bool useWrap)
        {
            string name = expr.GetType().Name;
            switch (name)
            {
                case "ArithmeticPairOp": this.SerializeArithmeticPairOp((ArithmeticPairOp)expr, useWrap); break;
                case "BooleanConstant": this.SerializeBooleanConstant((BooleanConstant)expr, useWrap); break;
                case "FunctionInvocation": this.SerializeFunctionInvocation((FunctionInvocation)expr, useWrap); break;
                case "IntegerConstant": this.SerializeIntegerConstant((IntegerConstant)expr, useWrap); break;
                case "LocalFunctionInvocation": this.SerializeLocalFunctionInvocation((LocalFunctionInvocation)expr, useWrap); break;
                case "MapAccess": this.SerializeMapAccess((MapAccess)expr, useWrap); break;
                case "PairComparison": this.SerializePairComparision((PairComparison)expr, useWrap); break;
                case "StringConstant": this.SerializeStringConstant((StringConstant)expr, useWrap); break;
                case "SystemFunctionInvocation": this.SerializeSystemFunctionInvocation((SystemFunctionInvocation)expr, useWrap); break;
                case "Variable": this.SerializeVariable((Variable)expr, useWrap); break;

                case "OpChain": throw new Exception(); // OpChains should be resolved at this point into more specific types

                default: throw new NotImplementedException();
            }
        }

        protected void SerializeExecutables(IList<Executable> lines)
        {
            foreach (Executable line in lines)
            {
                SerializeExecutable(line, false);
            }
        }

        private void SerializeFunctionInvocation(FunctionInvocation inv, bool useWrap)
        {
            Type funcType = inv.Root.ResolvedType;
            Type funcReturnType = funcType.Generics[0];
            Expression[] args = inv.ArgList;

            SystemFunction sf = inv.Root as SystemFunction;
            if (sf != null)
            {
                switch (sf.Name)
                {
                    case "array.castFrom":
                        {
                            Type targetItemType = funcReturnType.Generics[0];
                            Type targetType = funcType.Generics[1];
                            Type originalItemType = targetType.Generics[0];
                            this.SerializeSysFuncArrayCastFrom(targetItemType, originalItemType, targetType.IsArray, args[0], useWrap);
                        }
                        return;

                    case "list.of":
                        {
                            Type targetItemType = funcReturnType.Generics[0];
                            this.SerializeSysFuncListOf(targetItemType, args, useWrap);
                        }
                        return;

                    case "map.of":
                        {
                            Type keyType = funcReturnType.Generics[0];
                            Type valueType = funcReturnType.Generics[1];
                            this.SerializeSysFuncMapOf(keyType, valueType, args, useWrap);
                        }
                        return;

                    default:
                        throw new System.NotImplementedException();
                }
            }
            else
            {
                this.SerializeBasicFunctionInvocation(inv.Root, inv.ArgList, useWrap);
            }
        }

        private void SerializeOpChain(OpChain oc, bool useWrap)
        {
            throw new Exception(); // OpChains should be resolved at this point into more specific types
        }

        private void SerializeSystemFunctionInvocation(SystemFunctionInvocation sfi, bool useWrap)
        {
            switch (sfi.SysFuncId)
            {
                case SystemFunctionType.ARRAY_CAST_FROM: this.SerializeSysFuncArrayCastFrom(sfi.ResolvedType.ItemType, sfi.ArgList[0].ResolvedType.ItemType, sfi.ArgList[0].ResolvedType.IsArray, sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.FLOOR: this.SerializeSysFuncFloor(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.LIST_ADD: this.SerializeSysFuncListAdd(sfi.ArgList[0].ResolvedType.ItemType, sfi.ArgList[0], sfi.ArgList[1], useWrap); break;
                case SystemFunctionType.LIST_OF: this.SerializeSysFuncListOf(sfi.ResolvedType.ItemType, sfi.ArgList, useWrap); break;
                case SystemFunctionType.LIST_TO_ARRAY: this.SerializeSysFuncListToArray(sfi.ResolvedType.ItemType, sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.MAP_OF: this.SerializeSysFuncMapOf(sfi.ResolvedType.KeyType, sfi.ResolvedType.ValueType, sfi.ArgList, useWrap); break;
                case SystemFunctionType.SQRT: this.SerializeSysFuncSqrt(sfi.ArgList[0], useWrap); break;

                default: throw new NotImplementedException();
            }
        }

        private ClassDefinition[] GetClassesInDependencyOrder(ParseBundle bundle)
        {
            List<ClassDefinition> classes = new List<ClassDefinition>(bundle.ClassDefinitions.OrderBy(c => c.Name));
            List<ClassDefinition> classesInOrder = new List<ClassDefinition>();
            Dictionary<ClassDefinition, int> visitationState = new Dictionary<ClassDefinition, int>();
            foreach (ClassDefinition cd in classes)
            {
                visitationState[cd] = 0; // not visited
            }
            Dictionary<string, ClassDefinition> classLookup = classes.ToDictionary(c => c.Name);
            foreach (ClassDefinition cd in classes)
            {
                TraverseClasses(cd, classLookup, visitationState, classesInOrder);
            }
            return classesInOrder.ToArray();
        }

        private void TraverseClasses(ClassDefinition cd, Dictionary<string, ClassDefinition> classLookup, Dictionary<ClassDefinition, int> visitationState, List<ClassDefinition> classesOut)
        {
            if (visitationState[cd] == 2) return; // already serialized
            if (visitationState[cd] == 1) throw new ParserException(cd.FirstToken, "The inheritance hierarchy of this class creates a loop.");
            visitationState[cd] = 1;
            // TODO: add base class support and then check to see if there are base classes here.
            visitationState[cd] = 2;
            classesOut.Add(cd);
        }
    }
}
