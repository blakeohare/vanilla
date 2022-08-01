using System.Collections.Generic;
using System.Text;
using Vanilla.ParseTree;

namespace Vanilla.Transpiler
{
    internal class JavaScriptTranspiler : AbstractTranspiler
    {
        private bool standardWhitespaceEnabled = true;
        private int autoVarIdAlloc = 0;

        public JavaScriptTranspiler(ParseBundle bundle) : base(bundle)
        { }

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

        public override void EmitFiles(string verifiedDestinationPath)
        {
            string outputFile = this.GenerateMainFile();

            throw new System.NotImplementedException();
        }

        protected override void SerializeArithmeticPairOp(ArithmeticPairOp apo, bool useWrap)
        {
            bool isMod = apo.IsModulo;
            bool useNativeMod = isMod && apo.IsNativeModuloOkay;
            bool usingHelperFunction = isMod && !useNativeMod; // Add other examples here.

            if (useWrap)
            {
                Append(apo.ResolvedType.IsFloat ? "vutilGetFloat(vctx, " : "vutilGetInt(vctx, ");
            }
            else if (!usingHelperFunction)
            {
                Append('(');
            }

            if (isMod)
            {
                if (useNativeMod)
                {
                    SerializeExpression(apo.Left, false);
                    Append(" % ");
                    SerializeExpression(apo.Right, false);
                }
                else
                {
                    Append(apo.ResolvedType.IsFloat ? "vutilSafeModF(" : "vutilSafeMod(");
                    SerializeExpression(apo.Left, false);
                    Append(", ");
                    SerializeExpression(apo.Right, false);
                    Append(")");
                }
            }
            else
            {
                SerializeExpression(apo.Left, false);
                Append(' ');
                Append(apo.Op.Value);
                Append(' ');
                SerializeExpression(apo.Right, false);
            }

            if (useWrap || !usingHelperFunction) Append(')');
        }

        protected override void SerializeAssignment(Assignment asgn, bool omitSemicolon)
        {
            string op = asgn.Op.Value;

            if (!omitSemicolon) ApplyExecPrefix();
            if (asgn.Target is Variable v)
            {
                Append(v.Name);
                Append(" = ");

                if (op == "=")
                {
                    SerializeExpression(asgn.Value, true);
                }
                else
                {
                    op = op.Substring(0, op.Length - 1); // trim off the = suffix
                    string rootType = asgn.Value.ResolvedType.RootType;
                    if (rootType == "int")
                    {
                        Append("vutilGetInt(");
                    }
                    else if (rootType == "float")
                    {
                        Append("vutilGetFloat(");
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                    Append(v.Name);
                    Append(".value " + op + " (");
                    SerializeExpression(asgn.Value, false);
                    Append("))");
                }
            }
            else if (asgn.Target is MapAccess)
            {
                if (op != "=") throw new System.NotImplementedException(); // TODO: temporary storage of root expression and key computation if not a direct variable or constants.

                MapAccess ma = (MapAccess)asgn.Target;
                Expression key = ma.Key;
                SerializeExpression(ma.Root, false);
                Append('[');
                SerializeExpression(key, false);
                Append("] = ");
                SerializeExpression(asgn.Value, true);
            }
            else
            {
                throw new System.NotImplementedException();
            }
            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeBooleanConstant(BooleanConstant bc, bool useWrap)
        {
            if (useWrap)
            {
                Append("vctx.");
                Append(bc.Value ? "constTrue" : "constFalse");
            }
            else
            {
                Append(bc.Value ? "true" : "false");
            }
        }

        protected override void SerializeBasicFunctionInvocation(Expression root, Expression[] args, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeExpressionAsExecutable(ExpressionAsExecutable exex, bool omitSemicolon)
        {
            ApplyExecPrefix();
            // Use wrap since this is likely a function and functions use wrap be default and 
            // no wrap means there'll be useless code unwrapping it. If this becomes a problem 
            // then perhaps check the parse tree type of expr.
            SerializeExpression(exex.Expression, true);
            if (!omitSemicolon)
            {
                ApplyExecSuffix();
            }
        }

        protected override void SerializeForLoop(ForLoop floop)
        {
            ApplyExecPrefix();
            Append("for (");

            for (int i = 0; i < floop.Init.Length; i++)
            {
                if (i > 0) Append(", ");
                SerializeExecutable(floop.Init[i], true);
            }
            Append("; ");

            SerializeExpression(floop.Condition, false);
            Append("; ");
            for (int i = 0; i < floop.Step.Length; i++)
            {
                if (i > 0) Append(", ");
                SerializeExecutable(floop.Step[i], true);
            }
            Append(") {");
            Append(this.NL);
            this.TabLevel++;
            SerializeExecutables(floop.Code);
            this.TabLevel--;
            Append(this.NL);
            Append(this.CurrentTab);
            Append('}');
            Append(this.NL);
        }

        protected override void SerializeForRangeLoop(ForRangeLoop frl)
        {
            string iteratorVar = "$v" + autoVarIdAlloc++;
            string limitVar = "$v" + autoVarIdAlloc++;
            string stepVar = "$v" + autoVarIdAlloc++;

            ApplyExecPrefix();
            Append("let ");
            Append(iteratorVar);
            Append(" = ");
            SerializeExpression(frl.StartExpression, false);
            ApplyExecSuffix();

            ApplyExecPrefix();
            Append("let ");
            Append(limitVar);
            Append(" = ");
            SerializeExpression(frl.EndExpression, false);
            ApplyExecSuffix();

            ApplyExecPrefix();
            Append("const ");
            Append(stepVar);
            Append(" = ");
            Append(iteratorVar);
            Append(" <= ");
            Append(limitVar);
            Append(" ? 1 : -1");
            ApplyExecSuffix();

            if (frl.IsInclusive)
            {
                ApplyExecPrefix();
                Append(limitVar);
                Append(" += ");
                Append(stepVar);
                ApplyExecSuffix();
            }

            ApplyExecPrefix();
            Append("while (");
            Append(iteratorVar);
            Append(" != ");
            Append(limitVar);
            Append(") {");
            Append(this.NL);
            this.TabLevel++;

            ApplyExecPrefix();
            Append("const ");
            Append(frl.VarDeclaration.Name);
            Append(" = vutilGetInt(");
            Append(iteratorVar);
            Append(")");
            ApplyExecSuffix();

            SerializeExecutables(frl.Code);

            ApplyExecPrefix();
            Append(iteratorVar);
            Append(" += ");
            Append(stepVar);
            ApplyExecSuffix();

            this.TabLevel--;
            Append(this.NL);
            Append(this.CurrentTab);
            Append('}');
            Append(this.NL);
        }

        protected override void SerializeFunction(FunctionDefinition fd)
        {
            ApplyExecPrefix();
            Append("const ");
            Append(fd.Name);

            Append(" = (");
            for (int i = 0; i < fd.Args.Length; i++)
            {
                if (i > 0) Append(", ");
                Append(fd.Args[i].Name);
            }
            Append(") => {");
            Append(this.NL);
            this.TabLevel++;
            SerializeExecutables(fd.Body);
            this.TabLevel--;
            Append(this.CurrentTab);
            Append("};");
            Append(this.NL);
            Append(this.NL);
        }

        protected override void SerializeFunctionSignature(FunctionDefinition fd)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeIfStatement(IfStatement ifst)
        {
            ApplyExecPrefix();
            Append("if (");
            SerializeExpression(ifst.Condition, false);
            Append(") {");
            Append(this.NL);
            this.TabLevel++;
            SerializeExecutables(ifst.TrueCode);
            this.TabLevel--;
            Append(this.CurrentTab);
            Append('}');
            if (ifst.FalseCode.Length > 0)
            {
                // TODO: check for if else chains
                Append(" else {");
                Append(this.NL);
                this.TabLevel++;
                SerializeExecutables(ifst.FalseCode);
                this.TabLevel--;
                Append(this.CurrentTab);
                Append('}');
            }
            Append(this.NL);
        }

        protected override void SerializeIntegerConstant(IntegerConstant ic, bool useWrap)
        {
            if (useWrap)
            {
                Append("vutilGetInt(");
                Append(ic.Value + ")");
            }
            else
            {
                Append(ic.Value + "");
            }
        }

        protected override void SerializeLocalFunctionInvocation(LocalFunctionInvocation lfi, bool useWrap)
        {
            Append(lfi.FuncRef.FunctionDefinition.Name);
            Append('(');
            for (int i = 0; i < lfi.ArgList.Length; i++)
            {
                if (i > 0) Append(", ");
                SerializeExpression(lfi.ArgList[i], true);
            }
            Append(')');
            if (!useWrap)
            {
                Append(".value");
            }
        }

        protected override void SerializeMapAccess(MapAccess ma, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializePairComparision(PairComparison pc, bool useWrap)
        {
            Append('(');
            SerializeExpression(pc.Left, false);
            Append(") ");
            Append(pc.Op.Value);
            Append(" (");
            SerializeExpression(pc.Right, false);
            Append(')');
        }

        protected override void SerializeReturnStatement(ReturnStatement rs)
        {
            ApplyExecPrefix();
            Append("return ");
            SerializeExpression(rs.Value, true);
            ApplyExecSuffix();
        }

        protected override void SerializeStringConstant(StringConstant sc, bool useWrap)
        {
            // TODO: this is a terrible hack
            string codeValue = "'" + sc.Value.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
            if (useWrap)
            {
                // TODO: build a string table, like in the C version
                Append("vutilGetCommonString('");
                Append(codeValue);
                Append("')");
            }
            else
            {
                Append(codeValue);
            }
        }

        protected override void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection, bool useWrap)
        {
            if (useWrap)
            {
                Append("vutilWrapArray(");
            }
            Append('(');
            SerializeExpression(originalCollection, false);
            Append(").slice(0)");
            if (useWrap)
            {
                Append(')');
            }
        }

        protected override void SerializeSysFuncFloor(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutilGetInt(");
            Append("Math.floor(");
            SerializeExpression(expr, false);
            Append(")");
            if (useWrap) Append(')');
        }

        protected override void SerializeSysFuncListAdd(Type itemType, Expression listExpr, Expression itemExpr, bool useWrap)
        {
            Append('(');
            SerializeExpression(listExpr, false);
            Append(").push(");
            SerializeExpression(itemExpr, true);
            Append(")");
        }

        protected override void SerializeSysFuncListOf(Type itemType, Expression[] args, bool useWrap)
        {
            if (useWrap) Append("vutilWrapArray(");
            Append('[');
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) Append(", ");
                SerializeExpression(args[i], true);
            }
            Append(']');
            if (useWrap) Append(')');
        }

        protected override void SerializeSysFuncListToArray(Type itemType, Expression listExpr, bool useWrap)
        {
            if (useWrap) Append("vutilWrapArray(");
            SerializeExpression(listExpr, false);
            Append(".slice(0)");
            if (useWrap) Append(')');
        }

        protected override void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args, bool useWrap)
        {
            if (args.Length > 0)
            {
                throw new System.NotImplementedException();
            }

            if (useWrap)
            {
                Append("vutilNewMap({})");
            }
            else
            {
                Append("{}");
            }
        }

        protected override void SerializeSysFuncSqrt(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutilGetFloat(");
            Append("Math.sqrt(");
            SerializeExpression(expr, false);
            Append(')');
            if (useWrap) Append(')');
        }

        protected override void SerializeVariable(Variable vd, bool useWrap)
        {
            if (useWrap)
            {
                Append(vd.Name);
            }
            else
            {
                Append(vd.Name);
                Append(".value");
            }
        }

        protected override void SerializeVariableDeclaration(VariableDeclaration vd, bool omitSemicolon)
        {
            if (!omitSemicolon) ApplyExecPrefix();
            Append("let ");
            Append(vd.Name);
            Append(" = ");
            if (vd.InitialValue == null)
            {
                Append("vctx.globalNull");
            }
            else
            {
                SerializeExpression(vd.InitialValue, true);
            }

            if (!omitSemicolon) ApplyExecSuffix();
        }
    }
}
