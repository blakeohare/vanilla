using System.Collections.Generic;
using Vanilla.ParseTree;

namespace Vanilla.Transpiler
{
    internal class CTranspiler : AbstractTranspiler
    {
        private Dictionary<string, int> stringTableId = new Dictionary<string, int>();
        private bool standardWhitespaceEnabled = true;
        private int localVarId = 1;

        public CTranspiler(ParseBundle bundle) : base(bundle)
        {
            this.RequiresSignatureDeclaration = true;
        }

        private void ApplyExecPrefix()
        {
            if (standardWhitespaceEnabled)
            {
                Append(CurrentTab);
            }
        }

        private void ApplyExecSuffix()
        {
            if (standardWhitespaceEnabled)
            {
                Append(";");
                Append(NL);
            }
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

        protected override void SerializeAssignment(Assignment asgn)
        {
            ApplyExecPrefix();
            SerializeExpression(asgn.Target);
            Append(' ');
            Append(asgn.Op.Value);
            Append(' ');
            SerializeExpression(asgn.Value);
            ApplyExecSuffix();
        }

        protected override void SerializeVariableDeclaration(VariableDeclaration vd)
        {
            ApplyExecPrefix();
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
            ApplyExecSuffix();
        }

        protected override void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args)
        {
            Append("vutil_new_map('");
            switch (keyType.RootType)
            {
                case "string": Append('S'); break;
                case "int": Append('I'); break;
                default: Append('P'); break;
            }
            Append("')");
            for (int i = 0; i < args.Length; i += 2)
            {
                Expression key = args[i];
                Expression value = args[i];
                Append("->add_item(");
                SerializeExpression(key);
                Append(", ");
                SerializeExpression(value);
                Append(")");
            }
        }

        protected override void SerializeBasicFunctionInvocation(Expression root, Expression[] args)
        {
            if (root is FunctionReference)
            {
                Append(((FunctionReference)root).FunctionDefinition.Name);
                Append('(');
                for (int i = 0; i < args.Length; i++)
                {
                    this.SerializeExpression(args[i]);
                }
                Append(')');
            }
            else
            {
                throw new ParserException(root, "Not implemented yet");
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

        protected override void SerializeMapAccess(MapAccess ma)
        {
            Append("vutil_map_get_unsafe(");
            SerializeExpression(ma.Root);
            Append(", ");
            SerializeExpression(ma.Key);
            Append(")");
        }

        protected override void SerializeStringConstant(StringConstant sc)
        {
            Append("vctx->string_table[");
            if (!stringTableId.ContainsKey(sc.Value))
            {
                stringTableId[sc.Value] = stringTableId.Count;
            }
            Append(stringTableId[sc.Value] + "");
            Append("]");
        }

        protected override void SerializeBooleanConstant(BooleanConstant bc)
        {
            Append("vctx->");
            Append(bc.Value ? "const_true" : "const_false");
        }

        protected override void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection)
        {
            Append("vutil_list_clone(");
            SerializeExpression(originalCollection);
            Append(")");
        }

        protected override void SerializeReturnStatement(ReturnStatement rs)
        {
            ApplyExecPrefix();
            Append("return");
            if (rs.Value != null)
            {
                Append(' ');
                SerializeExpression(rs.Value);
            }
            ApplyExecSuffix();
        }

        protected override void SerializeSysFuncListOf(Type itemType, Expression[] args)
        {
            // lol
            foreach (Expression arg in args)
            {
                Append("vutil_list_add(");
            }
            Append("vutil_list_new()");
            foreach (Expression arg in args)
            {
                Append(", ");
                SerializeExpression(arg);
                Append(')');
            }
        }

        protected override void SerializeForRangeLoop(ForRangeLoop frl)
        {
            string startVarName = "_loc" + localVarId++;
            string endVarName = "_loc" + localVarId++;
            string stepVarName = "_loc" + localVarId++;
            string iteratorVarName = "_loc" + localVarId++;

            ApplyExecPrefix();
            Append("int ");
            Append(startVarName);
            Append(" = vutil_value_int(");
            SerializeExpression(frl.StartExpression);
            ApplyExecSuffix();

            ApplyExecPrefix();
            Append("int ");
            Append(endVarName);
            Append(" = vutil_value_int(");
            SerializeExpression(frl.EndExpression);
            ApplyExecSuffix();

            ApplyExecPrefix();
            Append("int ");
            Append(stepVarName);
            Append(" = ");
            Append(startVarName);
            Append(" < ");
            Append(endVarName);
            Append(" ? 1 : -1");
            ApplyExecSuffix();

            if (frl.IsInclusive)
            {
                ApplyExecPrefix();
                Append(endVarName);
                Append(" += ");
                Append(stepVarName);
                ApplyExecSuffix();
            }

            SerializeExecutable(frl.VarDeclaration);

            ApplyExecPrefix();
            Append("for (int ");
            Append(iteratorVarName);
            Append(" = ");
            Append(startVarName);
            Append("; ");
            Append(iteratorVarName);
            Append(" != ");
            Append(endVarName);
            Append("; ");
            Append(iteratorVarName);
            Append(" += ");
            Append(stepVarName);
            Append(") {");
            Append(NL);

            this.TabLevel++;

            ApplyExecPrefix();
            Append("_v" + frl.VarDeclaration.Name);
            Append(" = vutil_int(ctx, ");
            Append(iteratorVarName);
            Append(");");
            Append(NL);

            SerializeExecutables(frl.Code);

            this.TabLevel--;
            Append(CurrentTab);
            Append('}');
            Append(NL);
        }

        protected override void SerializeIfStatement(IfStatement ifst)
        {
            this.SerializeIfStatement(ifst, false);
        }

        private void SerializeIfStatement(IfStatement ifst, bool isNested)
        {
            if (!isNested) ApplyExecPrefix();
            Append("if (vutil_value_bool(");
            SerializeExpression(ifst.Condition);
            Append(") {");
            Append(NL);
            this.TabLevel++;
            SerializeExecutables(ifst.TrueCode);
            this.TabLevel--;
            Append(CurrentTab);
            Append("}");
            if (ifst.FalseCode != null && ifst.FalseCode.Length > 0)
            {
                Append(" else ");
                if (ifst.FalseCode.Length == 1 && ifst.FalseCode[0] is IfStatement)
                {
                    SerializeIfStatement((IfStatement)ifst.FalseCode[0], true);
                }
                else
                {
                    Append("{");
                    Append(NL);
                    this.TabLevel++;
                    SerializeExecutables(ifst.FalseCode);
                    this.TabLevel--;
                    Append("}");
                }
            }
            if (!isNested) Append(NL);
        }

        protected override void SerializeExpressionAsExecutable(ExpressionAsExecutable exex)
        {
            ApplyExecPrefix();
            SerializeExpression(exex.Expression);
            ApplyExecSuffix();
        }
    }
}
