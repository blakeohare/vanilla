using System.Collections.Generic;
using System.Linq;
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

        public override void EmitFiles(string verifiedDestinationPath)
        {
            string destDir = verifiedDestinationPath;
            string outputFile = this.GenerateMainFile();
            List<string> outputFilePieces = new List<string>() {
                "#include <stdio.h>",
                "#include <stdlib.h>",
                "#include \"gen_util.h\"",
                ""
            };

            string[] stringTableMembers = this.GetStringTableEntries();

            outputFilePieces.AddRange(new string[] {
                "VContext* create_context() {",
                "    VContext* vctx = vutil_initialize_context(" + stringTableMembers.Length + ");",
            });
            for (int i = 0; i < stringTableMembers.Length; i++)
            {
                string escapedString = '"' + stringTableMembers[i].Replace("\"", "\\\"") + '"'; // TODO: This is a hack just to get it compiling. Need to escape strings with LATIN-only characters. Then need to create an alternative factory method for strings with special characters.
                outputFilePieces.Add("    vctx->string_table[" + i + "] = vutil_get_string_from_chars(vctx, " + escapedString + ");");
            }
            outputFilePieces.Add("    return vctx;");
            outputFilePieces.Add("}");

            outputFilePieces.Add("");
            outputFilePieces.Add(outputFile);

            outputFile = WrapWithIfNDef("_VANILLA_GEN_USER_CODE_H", string.Join('\n', outputFilePieces));

            System.IO.File.WriteAllText(System.IO.Path.Combine(destDir, "gen.h"), EnsureLineEndings(outputFile));

            string[] headerFileContent = new string[] {
                "value.h",
                "vcontext.h",
                "util.h",
            }.Select(name => Resources.GetResourceText("Transpiler/Support/C/" + name)).ToArray();

            string code = string.Join("\n\n", headerFileContent);

            System.IO.File.WriteAllText(
                System.IO.Path.Combine(destDir, "gen_util.h"),
                EnsureLineEndings(WrapWithIfNDef("_VANILLA_GENERATED_UTIL_H",
                    "#include <stdio.h>\n" +
                    "#include <stdlib.h>\n" +
                    "#include <math.h>\n" +
                    "\n" +
                    code
                    )));
        }

        private static string EnsureLineEndings(string code)
        {
            return code.Replace("\r\n", "\n");
        }

        private static string WrapWithIfNDef(string defVar, string code)
        {
            return string.Join("\n", new string[] {
                    "#ifndef " + defVar,
                    "#define " + defVar,
                    "",
                    code.Trim(),
                    "",
                    "#endif // " + defVar,
                    "",
                });
        }

        private void EnsureUsingWrap(bool useWrap)
        {
            if (!useWrap) throw new System.Exception("Not using wrap here doesn't make sense.");
        }

        private void ApplyUnwrapPrefix(Expression expression, bool useWrap)
        {
            if (useWrap) return;
            Type outgoingType = expression.ResolvedType;
            switch (outgoingType.RootType)
            {
                case "bool":
                    Append("((ValueBoolean*)(");
                    break;
                case "int":
                    Append("((ValueInt*)(");
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        private void ApplyUnwrapSuffix(Expression expression, bool useWrap)
        {
            if (useWrap) return;
            Type outgoingType = expression.ResolvedType;
            switch (outgoingType.RootType)
            {
                case "bool":
                case "int":
                    Append("))->value");
                    break;
                default:
                    throw new System.NotImplementedException();
            }
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
            Append("(VContext* vctx");
            for (int i = 0; i < fd.Args.Length; i++)
            {
                Append(", ");
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
            Append("(VContext* vctx");
            for (int i = 0; i < fd.Args.Length; i++)
            {
                Append(", ");
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

        protected override void SerializeGetFieldValue(DotField df, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeAssignmentToField(DotField df, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();
            throw new System.NotImplementedException();
            // if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeAssignmentToMap(MapAccess ma, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();
            if (op != "=") throw new System.NotImplementedException(); // TODO: temporary storage of root expression and key computation if not a direct variable or constants.

            Expression key = ma.Key;
            Append(key.ResolvedType.IsString ? "vutil_map_set_str(" : "vutil_map_set_int(");
            SerializeExpression(ma.Root, true);
            Append(", ");
            SerializeExpression(key, true);
            Append(", ");
            SerializeExpression(asgn.Value, true);
            Append(')');
            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeAssignmentToVariable(Variable v, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();
            if (op == "=")
            {
                Append(v.Name);
                Append(' ');
                Append(op);
                Append(' ');
                SerializeExpression(asgn.Value, true);
            }
            else
            {
                Append(v.Name);
                Append(" = ");
                switch (asgn.Target.ResolvedType.RootType)
                {
                    case "int":
                        Append("vutil_get_int(vctx, ");
                        break;
                    default: throw new System.NotImplementedException();
                }
                SerializeVariable(v, false);
                Append(op[0]);
                SerializeExpression(asgn.Value, false);
                Append(')');
            }
            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeVariableDeclaration(VariableDeclaration vd, bool omitSemicolon)
        {
            if (!omitSemicolon) ApplyExecPrefix();
            Append("Value* ");
            Append(vd.Name);
            if (vd.InitialValue == null)
            {
                Append(" = NULL");
            }
            else
            {
                Append(" = ");
                SerializeExpression(vd.InitialValue, true);
            }
            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeSysFuncMapOf(Type keyType, Type valueType, Expression[] args, bool useWrap)
        {
            EnsureUsingWrap(useWrap);

            Append("vutil_new_map(vctx, ");
            Append(keyType.IsString ? '1' : '0');
            Append(')');
            for (int i = 0; i < args.Length; i += 2)
            {
                Expression key = args[i];
                Expression value = args[i];
                Append("->add_item(");
                SerializeExpression(key, true);
                Append(", ");
                SerializeExpression(value, true);
                Append(")");
            }
        }

        protected override void SerializeMethodInvocation(Expression root, string methodName, Expression[] args, bool useWrap)
        {
            if (root is FunctionReference)
            {
                Append(((FunctionReference)root).FunctionDefinition.Name);
                Append('(');
                for (int i = 0; i < args.Length; i++)
                {
                    this.SerializeExpression(args[i], true);
                }
                Append(')');
            }
            else
            {
                throw new ParserException(root, "Not implemented yet");
            }
        }

        protected override void SerializeConstructor(ConstructorDefinition ctor)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeConstructorInvocation(ConstructorInvocation ctorInvoke)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeIntegerConstant(IntegerConstant ic, bool useWrap)
        {
            if (useWrap)
            {
                Append("vutil_get_int(vctx, " + ic.Value + ")");
            }
            else
            {
                Append("" + ic.Value);
            }
        }

        protected override void SerializeVariable(Variable vd, bool useWrap)
        {
            ApplyUnwrapPrefix(vd, useWrap);
            Append(vd.Name);
            ApplyUnwrapSuffix(vd, useWrap);
        }

        protected override void SerializeMapAccess(MapAccess ma, bool useWrap)
        {
            ApplyUnwrapPrefix(ma, useWrap);
            Append("vutil_map_get_unsafe(");
            SerializeExpression(ma.Root, true);
            Append(", ");
            SerializeExpression(ma.Key, true);
            Append(")");
            ApplyUnwrapSuffix(ma, useWrap);
        }

        protected override void SerializeNullConstant(bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeStringConstant(StringConstant sc, bool useWrap)
        {
            ApplyUnwrapPrefix(sc, useWrap);
            Append("vctx->string_table[");
            if (!stringTableId.ContainsKey(sc.Value))
            {
                stringTableId[sc.Value] = stringTableId.Count;
            }
            Append(stringTableId[sc.Value] + "");
            Append("]");
            ApplyUnwrapSuffix(sc, useWrap);
        }

        protected override void SerializeThisConstant(ThisConstant thiz, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeBooleanConstant(BooleanConstant bc, bool useWrap)
        {
            ApplyUnwrapPrefix(bc, useWrap);
            Append("vctx->");
            Append(bc.Value ? "global_true" : "global_false");
            ApplyUnwrapSuffix(bc, useWrap);
        }

        protected override void SerializeSysFuncArrayCastFrom(Type targetItemType, Type sourceItemType, bool isArray, Expression originalCollection, bool useWrap)
        {
            EnsureUsingWrap(useWrap);
            Append("vutil_list_clone(vctx, ");
            SerializeExpression(originalCollection, true);
            Append(")");
        }

        protected override void SerializeReturnStatement(ReturnStatement rs)
        {
            ApplyExecPrefix();
            Append("return");
            if (rs.Value != null)
            {
                Append(' ');
                SerializeExpression(rs.Value, true);
            }
            ApplyExecSuffix();
        }

        protected override void SerializeSysFuncListOf(Type itemType, Expression[] args, bool useWrap)
        {
            EnsureUsingWrap(useWrap);

            // lol
            foreach (Expression arg in args)
            {
                Append("vutil_list_add(vctx, ");
            }
            Append("vutil_list_new(vctx)");
            foreach (Expression arg in args)
            {
                Append(", ");
                SerializeExpression(arg, true);
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
            Append(" = ");
            SerializeExpression(frl.StartExpression, false);
            ApplyExecSuffix();

            ApplyExecPrefix();
            Append("int ");
            Append(endVarName);
            Append(" = ");
            SerializeExpression(frl.EndExpression, false);
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

            SerializeExecutable(frl.VarDeclaration, false);

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
            Append(frl.VarDeclaration.Name);
            Append(" = vutil_get_int(vctx, ");
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

        protected override void SerializeForLoop(ForLoop floop)
        {
            foreach (Executable initEx in floop.Init)
            {
                SerializeExecutable(initEx, false);
            }

            ApplyExecPrefix();
            Append("while (");
            if (floop.Condition != null)
            {
                SerializeExpression(floop.Condition, false);
            }
            else
            {
                Append('1');
            }
            Append(") {");
            Append(NL);
            this.TabLevel++;
            SerializeExecutables(floop.Code);
            Append(NL);
            if (floop.Step.Length > 0)
            {
                SerializeExecutables(floop.Step);
            }
            this.TabLevel--;
            Append(CurrentTab);
            Append("}");
            Append(NL);
        }

        private void SerializeIfStatement(IfStatement ifst, bool isNested)
        {
            if (!isNested) ApplyExecPrefix();
            Append("if (");
            SerializeExpression(ifst.Condition, false);
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

        protected override void SerializeExpressionAsExecutable(ExpressionAsExecutable exex, bool omitSemicolon)
        {
            if (!omitSemicolon) ApplyExecPrefix();
            SerializeExpression(exex.Expression, true);
            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeLocalFunctionInvocation(LocalFunctionInvocation lfi, bool useWrap)
        {
            ApplyUnwrapPrefix(lfi, useWrap);
            Append("fn_");
            Append(lfi.FuncRef.FunctionDefinition.Name);
            Append("(vctx");
            for (int i = 0; i < lfi.ArgList.Length; i++)
            {
                Append(", ");
                SerializeExpression(lfi.ArgList[i], true);
            }
            Append(')');
            ApplyUnwrapSuffix(lfi, useWrap);
        }

        protected override void SerializeSysFuncListAdd(Type itemType, Expression listExpr, Expression itemExpr, bool useWrap)
        {
            EnsureUsingWrap(useWrap); // void
            Append("vutil_list_add(vctx, ");
            SerializeExpression(listExpr, true);
            Append(", ");
            SerializeExpression(itemExpr, true);
            Append(')');
        }

        protected override void SerializeSysFuncListToArray(Type itemType, Expression listExpr, bool useWrap)
        {
            EnsureUsingWrap(useWrap);
            Append("vutil_list_clone(vctx, ");
            SerializeExpression(listExpr, true);
            Append(')');
        }

        protected override void SerializePairComparision(PairComparison pc, bool useWrap)
        {
            if (useWrap) Append("vutil_get_bool(vctx, ");
            Append('(');
            SerializeExpression(pc.Left, false);
            Append(") ");
            Append(pc.Op.Value);
            Append(" (");
            SerializeExpression(pc.Right, false);
            Append(")");
            if (useWrap) Append(')');
        }

        protected override void SerializeArithmeticPairOp(ArithmeticPairOp apo, bool useWrap)
        {
            bool isMod = apo.IsModulo;
            bool useNativeMod = isMod && apo.IsNativeModuloOkay;
            bool usingHelperFunction = isMod && !useNativeMod; // Add other examples here.

            if (useWrap)
            {
                Append(apo.ResolvedType.IsFloat ? "vutil_get_float(vctx, " : "vutil_get_int(vctx, ");
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
                    Append(apo.ResolvedType.IsFloat ? "vutil_safe_modf(" : "vutil_safe_mod(");
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

        protected override void SerializeSysFuncFloor(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutil_get_int(vctx, ");
            else Append(')');
            Append("(int) ");
            SerializeExpression(expr, false);
            Append(')');
        }

        protected override void SerializeSysFuncSqrt(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutil_get_float(vctx, ");
            Append("vutil_sqrt(");
            SerializeExpression(expr, false);
            Append(")");
            if (useWrap) Append(')');
        }

        protected override void SerializeSysFuncStringReplace(Expression str, Expression needle, Expression newValue, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeSysFuncStringToCharArray(Expression str, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeSysFuncStringTrim(Expression str, bool useWrap)
        {
            throw new System.NotImplementedException();
        }

        protected override void SerializeSysPropListLength(Expression expr, bool useWrap)
        {
            throw new System.NotImplementedException();
        }
    }
}
