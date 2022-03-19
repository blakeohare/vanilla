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

        public string GenerateFile()
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

        protected abstract void SerializeAssignment(Assignment asgn);
        protected abstract void SerializeBasicFunctionInvocation(Expression root, Expression[] args);
        protected abstract void SerializeBooleanConstant(BooleanConstant bc);
        protected abstract void SerializeExpressionAsExecutable(ExpressionAsExecutable exex);
        protected abstract void SerializeForRangeLoop(ForRangeLoop frl);
        protected abstract void SerializeFunction(FunctionDefinition fd);
        protected abstract void SerializeFunctionSignature(FunctionDefinition fd);
        protected abstract void SerializeIfStatement(IfStatement ifst);
        protected abstract void SerializeIntegerConstant(IntegerConstant ic);
        protected abstract void SerializeMapAccess(MapAccess ma);
        protected abstract void SerializeReturnStatement(ReturnStatement rs);
        protected abstract void SerializeStringConstant(StringConstant sc);
        protected abstract void SerializeVariable(Variable vd);
        protected abstract void SerializeVariableDeclaration(VariableDeclaration vd);
        protected abstract void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args);
        protected abstract void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection);
        protected abstract void SerializeSysFuncListOf(Type itemType, Expression[] args);

        internal void SerializeExecutable(Executable ex)
        {
            string name = ex.GetType().Name;
            switch (name)
            {
                case "Assignment": this.SerializeAssignment((Assignment)ex); break;
                case "ExpressionAsExecutable": this.SerializeExpressionAsExecutable((ExpressionAsExecutable)ex); break;
                case "IfStatement": this.SerializeIfStatement((IfStatement)ex); break;
                case "ReturnStatement": this.SerializeReturnStatement((ReturnStatement)ex); break;
                case "VariableDeclaration": this.SerializeVariableDeclaration((VariableDeclaration)ex); break;
                case "ForRangeLoop": this.SerializeForRangeLoop((ForRangeLoop)ex); break;
                default: throw new NotImplementedException(name);
            }
        }

        protected void SerializeExpression(Expression expr)
        {
            string name = expr.GetType().Name;
            switch (name)
            {
                case "BooleanConstant": this.SerializeBooleanConstant((BooleanConstant)expr); break;
                case "FunctionInvocation": this.SerializeFunctionInvocation((FunctionInvocation)expr); break;
                case "IntegerConstant": this.SerializeIntegerConstant((IntegerConstant)expr); break;
                case "MapAccess": this.SerializeMapAccess((MapAccess)expr); break;
                case "StringConstant": this.SerializeStringConstant((StringConstant)expr); break;
                case "Variable": this.SerializeVariable((Variable)expr); break;
                default: throw new NotImplementedException();
            }
        }

        protected void SerializeExecutables(IList<Executable> lines)
        {
            foreach (Executable line in lines)
            {
                SerializeExecutable(line);
            }
        }

        private void SerializeFunctionInvocation(FunctionInvocation inv)
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
                            this.SerializeSysFuncArrayCastFrom(targetItemType, originalItemType, targetType.IsArray, args[0]);
                        }
                        return;

                    case "list.of":
                        {
                            Type targetItemType = funcReturnType.Generics[0];
                            this.SerializeSysFuncListOf(targetItemType, args);
                        }
                        return;

                    case "map.of":
                        {
                            Type keyType = funcReturnType.Generics[0];
                            Type valueType = funcReturnType.Generics[1];
                            this.SerializeSysFuncMapOf(keyType, valueType, args);
                        }
                        return;

                    default:
                        throw new System.NotImplementedException();
                }
            }
            else
            {
                this.SerializeBasicFunctionInvocation(inv.Root, inv.ArgList);
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