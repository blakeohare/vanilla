using Vanilla.ParseTree;

namespace Vanilla.Transpiler
{
    internal class CTranspiler : AbstractTranspiler
    {
        private bool standardWhitespaceEnabled = true;
        public CTranspiler(ParseBundle bundle) : base(bundle)
        {
            this.RequiresSignatureDeclaration = true;
        }

        protected override void SerializeFunctionSignature(FunctionDefinition fd)
        {
            Append(CurrentTab);
            Append("Value* fn_");
            Append(fd.Name);
            Append('(');
            for (int i = 0; i < fd.Args.Length; i++)
            {
                if (i > 0) Append(", ");
                Append("Value* ");
                Append(fd.Args[i].Name);
            }
            Append(");");
            Append(NL);
            Append(NL);
        }

        protected override void SerializeFunction(FunctionDefinition fd)
        {
            Append(CurrentTab);
            Append("Value* fn_");
            Append(fd.Name);
            Append('(');
            for (int i = 0; i < fd.Args.Length; i++)
            {
                if (i > 0) Append(", ");
                Append("Value* ");
                Append(fd.Args[i].Name);
            }
            Append(") {");
            Append(NL);
            this.TabLevel++;
            this.SerializeExecutables(fd.Body);
            this.TabLevel--;
            Append("}");
            Append(NL);
            Append(NL);
        }

        protected override void SerializeExecutable(Executable ex)
        {
            if (standardWhitespaceEnabled) Append(CurrentTab);
            string name = ex.GetType().Name;
            switch (name)
            {
                case "VariableDeclaration": this.SerializeVariableDeclaration((VariableDeclaration)ex); break;
                default: throw new System.NotImplementedException(name);
            }
            if (standardWhitespaceEnabled) Append(NL);
        }

        protected override void SerializeVariableDeclaration(VariableDeclaration vd)
        {
            Append("Value* ");
            Append(vd.Name);
            if (vd.InitialValue == null)
            {
                Append(" = NULL");
            }
            else
            {
                Append(" = ");
                SerializeExpression(vd.InitialValue);
            }
            if (standardWhitespaceEnabled) Append(';');
        }

        protected override void SerializeFunctionInvocation(FunctionInvocation inv)
        {
            if (inv.Root is SystemFunction)
            {
                throw new System.NotImplementedException();
            }
            else if (inv.Root is FunctionReference)
            {
                Append(((FunctionReference)inv.Root).FunctionDefinition.Name);
                Append('(');
                for (int i = 0; i < inv.ArgList.Length; i++)
                {
                    this.SerializeExpression(inv.ArgList[i]);
                }
                Append(')');
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        protected override void SerializeIntegerConstant(IntegerConstant ic)
        {
            Append("" + ic.Value);
        }

        protected override void SerializeVariable(Variable vd)
        {
            Append(vd.Name);
        }
    }
}
