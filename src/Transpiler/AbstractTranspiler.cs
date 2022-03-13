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

        protected abstract void SerializeExecutable(Executable ex);
        protected abstract void SerializeFunction(FunctionDefinition fd);
        protected abstract void SerializeFunctionSignature(FunctionDefinition fd);
        protected abstract void SerializeFunctionInvocation(FunctionInvocation inv);
        protected abstract void SerializeIntegerConstant(IntegerConstant ic);
        protected abstract void SerializeVariable(Variable vd);
        protected abstract void SerializeVariableDeclaration(VariableDeclaration vd);

        protected void SerializeExpression(Expression expr)
        {
            string name = expr.GetType().Name;
            switch (name)
            {
                case "FunctionInvocation": this.SerializeFunctionInvocation((FunctionInvocation)expr); break;
                case "IntegerConstant": this.SerializeIntegerConstant((IntegerConstant)expr); break;
                case "Variable": this.SerializeVariable((Variable)expr);break;
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
