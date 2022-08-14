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

        public string[] GetStringTableEntries()
        {
            return this.bundle.StringDefinitions.ToArray();
        }

        public ClassDefinition[] GetClassDefinitions()
        {
            return this.bundle.ClassDefinitions.ToArray();
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

            foreach (ClassDefinition cd in classes)
            {
                ConstructorDefinition ctor = cd.Members.OfType<ConstructorDefinition>().First();
                this.SerializeConstructor(ctor);
                foreach (FunctionDefinition method in cd.Members.OfType<FunctionDefinition>())
                {
                    this.SerializeFunction(method);
                }
            }

            foreach (FunctionDefinition fd in functions)
            {
                this.SerializeFunction(fd);
            }
            return this.sb.ToString();
        }

        protected abstract void SerializeArithmeticPairOp(ArithmeticPairOp apo, bool useWrap);
        protected abstract void SerializeArrayIndex(ArrayIndex arrIndex, bool useWrap);
        protected abstract void SerializeAssignmentToVariable(Variable target, Assignment asgn, bool omitSemicolon, bool floatCast);
        protected abstract void SerializeAssignmentToMap(MapAccess target, Assignment asgn, bool omitSemicolon, bool floatCast);
        protected abstract void SerializeAssignmentToField(FieldReference target, Assignment asgn, bool omitSemicolon, bool floatCast);
        protected abstract void SerializeBooleanCombination(BooleanCombination bc, bool useWrap);
        protected abstract void SerializeBooleanConstant(BooleanConstant bc, bool useWrap);
        protected abstract void SerializeConstructor(ConstructorDefinition ctor);
        protected abstract void SerializeConstructorInvocation(ConstructorInvocation ctorInvoke);
        protected abstract void SerializeExpressionAsExecutable(ExpressionAsExecutable exex, bool omitSemicolon);
        protected abstract void SerializeFieldReference(FieldReference fieldRef, bool useWrap);
        protected abstract void SerializeFloatCast(FloatCast fc, bool useWrap);
        protected abstract void SerializeForLoop(ForLoop floop);
        protected abstract void SerializeForRangeLoop(ForRangeLoop frl);
        protected abstract void SerializeFunction(FunctionDefinition fd);
        protected abstract void SerializeFunctionSignature(FunctionDefinition fd);
        protected abstract void SerializeGetFieldValue(DotField df, bool useWrap);
        protected abstract void SerializeIfStatement(IfStatement ifst);
        protected abstract void SerializeIntegerConstant(IntegerConstant ic, bool useWrap);
        protected abstract void SerializeLocalFunctionInvocation(LocalFunctionInvocation lfi, bool useWrap);
        protected abstract void SerializeMapAccess(MapAccess ma, bool useWrap);
        protected abstract void SerializeMethodInvocation(MethodInvocation mi, bool useWrap);
        protected abstract void SerializeNullConstant(bool useWrap);
        protected abstract void SerializePairComparision(PairComparison pc, bool useWrap);
        protected abstract void SerializeReturnStatement(ReturnStatement rs);
        protected abstract void SerializeStringComparison(Expression str1, Expression str2, bool useWrap);
        protected abstract void SerializeStringConcatChain(StringConcatChain strChain, bool useWrap);
        protected abstract void SerializeStringConstant(StringConstant sc, bool useWrap);
        protected abstract void SerializeThisConstant(ThisConstant thiz, bool useWrap);
        protected abstract void SerializeVariable(Variable vd, bool useWrap);
        protected abstract void SerializeVariableDeclaration(VariableDeclaration vd, bool omitSemicolon);

        protected abstract void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection, bool useWrap);
        protected abstract void SerializeSysFuncFloor(Expression expr, bool useWrap);
        protected abstract void SerializeSysFuncListAdd(Type itemType, Expression listExpr, Expression itemExpr, bool useWrap);
        protected abstract void SerializeSysFuncListLength(Expression expr, bool useWrap);
        protected abstract void SerializeSysFuncListOf(Type itemType, Expression[] args, bool useWrap);
        protected abstract void SerializeSysFuncListToArray(Type itemType, Expression listExpr, bool useWrap);
        protected abstract void SerializeSysFuncMapContains(Expression mapExpr, Expression keyExpr, bool useWrap);
        protected abstract void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args, bool useWrap);
        protected abstract void SerializeSysFuncSqrt(Expression expr, bool useWrap);
        protected abstract void SerializeSysFuncStringFirstCharCode(Expression str, bool useWrap);
        protected abstract void SerializeSysFuncStringReplace(Expression str, Expression needle, Expression newValue, bool useWrap);
        protected abstract void SerializeSysFuncStringToCharArray(Expression str, bool useWrap);
        protected abstract void SerializeSysFuncStringTrim(Expression str, bool useWrap);
        protected abstract void SerializeSysPropListLength(Expression expr, bool useWrap);

        internal void SerializeExecutable(Executable ex, bool omitSemicolon)
        {
            string name = ex.GetType().Name;
            switch (name)
            {
                case "Assignment":
                    // Maybe this should be in the resolver?
                    Assignment asgn = (Assignment)ex;
                    Type targetType = asgn.Target.ResolvedType;
                    Type valueType = asgn.Value.ResolvedType;
                    bool floatCast = targetType.IsFloat && valueType.IsInteger;
                    if (asgn.Target is Variable)
                    {
                        this.SerializeAssignmentToVariable((Variable)asgn.Target, asgn, omitSemicolon, floatCast);
                    }
                    else if (asgn.Target is MapAccess)
                    {
                        this.SerializeAssignmentToMap((MapAccess)asgn.Target, asgn, omitSemicolon, floatCast);
                    }
                    else if (asgn.Target is FieldReference)
                    {
                        this.SerializeAssignmentToField((FieldReference)asgn.Target, asgn, omitSemicolon, floatCast);
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
                case "ArrayIndex": this.SerializeArrayIndex((ArrayIndex)expr, useWrap); break;
                case "BooleanCombination": this.SerializeBooleanCombination((BooleanCombination)expr, useWrap); break;
                case "BooleanConstant": this.SerializeBooleanConstant((BooleanConstant)expr, useWrap); break;
                case "FieldReference": this.SerializeFieldReference((FieldReference)expr, useWrap); break;
                case "FloatCast": this.SerializeFloatCast((FloatCast)expr, useWrap); break;
                case "IntegerConstant": this.SerializeIntegerConstant((IntegerConstant)expr, useWrap); break;
                case "LocalFunctionInvocation": this.SerializeLocalFunctionInvocation((LocalFunctionInvocation)expr, useWrap); break;
                case "MapAccess": this.SerializeMapAccess((MapAccess)expr, useWrap); break;
                case "MethodInvocation": this.SerializeMethodInvocation((MethodInvocation)expr, useWrap); break;
                case "NullConstant": this.SerializeNullConstant(useWrap); break;
                case "StringConcatChain": this.SerializeStringConcatChain((StringConcatChain)expr, useWrap); break;
                case "StringConstant": this.SerializeStringConstant((StringConstant)expr, useWrap); break;
                case "SystemFunctionInvocation": this.SerializeSystemFunctionInvocation((SystemFunctionInvocation)expr, useWrap); break;
                case "ThisConstant": this.SerializeThisConstant((ThisConstant)expr, useWrap); break;
                case "Variable": this.SerializeVariable((Variable)expr, useWrap); break;

                case "PairComparison":
                    PairComparison pc = (PairComparison)expr;
                    if (pc.Left.ResolvedType.IsString && pc.Right.ResolvedType.IsString && pc.Op.Value == "==")
                    {
                        this.SerializeStringComparison(pc.Left, pc.Right, useWrap);
                    }
                    else
                    {
                        this.SerializePairComparision((PairComparison)expr, useWrap);
                    }
                    break;

                case "ConstructorInvocation":
                    if (!useWrap) throw new Exception(); // should not happen
                    this.SerializeConstructorInvocation((ConstructorInvocation)expr);
                    break;

                case "DotField":
                case "OpChain":
                case "SystemFunction":
                case "TypeRootedExpression":
                    // Valid usages of these should have been resolved into other nodes.
                    throw new ParserException(expr, "This type of expression is not allowed here.");

                default: throw new NotImplementedException(name);
            }
        }

        protected void SerializeExecutables(IList<Executable> lines)
        {
            foreach (Executable line in lines)
            {
                SerializeExecutable(line, false);
            }
        }

        private void SerializeSystemFunctionInvocation(SystemFunctionInvocation sfi, bool useWrap)
        {
            switch (sfi.SysFuncId)
            {
                case SystemFunctionType.ARRAY_CAST_FROM: this.SerializeSysFuncArrayCastFrom(sfi.ResolvedType.ItemType, sfi.ArgList[0].ResolvedType.ItemType, sfi.ArgList[0].ResolvedType.IsArray, sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.ARRAY_LENGTH: this.SerializeSysFuncListLength(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.FLOOR: this.SerializeSysFuncFloor(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.LIST_ADD: this.SerializeSysFuncListAdd(sfi.ArgList[0].ResolvedType.ItemType, sfi.ArgList[0], sfi.ArgList[1], useWrap); break;
                case SystemFunctionType.LIST_LENGTH: this.SerializeSysFuncListLength(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.LIST_OF: this.SerializeSysFuncListOf(sfi.ResolvedType.ItemType, sfi.ArgList, useWrap); break;
                case SystemFunctionType.LIST_TO_ARRAY: this.SerializeSysFuncListToArray(sfi.ResolvedType.ItemType, sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.MAP_CONTAINS: this.SerializeSysFuncMapContains(sfi.ArgList[0], sfi.ArgList[1], useWrap); break;
                case SystemFunctionType.MAP_OF: this.SerializeSysFuncMapOf(sfi.ResolvedType.KeyType, sfi.ResolvedType.ValueType, sfi.ArgList, useWrap); break;
                case SystemFunctionType.SQRT: this.SerializeSysFuncSqrt(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.STRING_FIRST_CHAR_CODE: this.SerializeSysFuncStringFirstCharCode(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.STRING_TO_CHARACTER_ARRAY: this.SerializeSysFuncStringToCharArray(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.STRING_TRIM: this.SerializeSysFuncStringTrim(sfi.ArgList[0], useWrap); break;
                case SystemFunctionType.STRING_REPLACE: this.SerializeSysFuncStringReplace(sfi.ArgList[0], sfi.ArgList[1], sfi.ArgList[2], useWrap); break;

                default: throw new NotImplementedException(sfi.SysFuncId.ToString());
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
