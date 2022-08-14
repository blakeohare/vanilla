using System.Collections.Generic;
using System.Linq;
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

            string destDir = verifiedDestinationPath;

            List<string> outputFilePieces = new List<string>();

            FunctionDefinition[] pubFuncs = this.GetPublicFunctions();

            outputFilePieces.Add("const { " + string.Join(", ", pubFuncs.Select(pb => pb.Name)) + " } = (() => {");
            outputFilePieces.Add(Resources.GetResourceText("Transpiler/Support/JavaScript/VContext.js"));
            outputFilePieces.Add(Resources.GetResourceText("Transpiler/Support/JavaScript/vutil.js"));

            string[] vutilMethods = new string[] {
                "vutilGetCommonString",
                "vutilGetInt",
                "vutilGetString",
                "vutilMapSet",
                "vutilNewInstance",
                "vutilNewMap",
                "vutilSafeMod",
                "vutilUnwrapNative",
                "vutilWrapArray",
                "vutilWrapNative",
            };

            outputFilePieces.Add(string.Join('\n', new string[] {
                "const vctx = createVanillaContext();",
                "const vutil = createVutil(vctx);",
                "const { " + string.Join(", ", vutilMethods) + " } = vutil;",
            }));

            outputFilePieces.Add(outputFile);

            List<string> methodBuffer = new List<string>();
            foreach (ClassDefinition cd in this.GetClassDefinitions())
            {
                methodBuffer.Add("vctx.classMetadata." + cd.Name + " = { methods: ");
                FunctionDefinition[] methods = cd.Members.OfType<FunctionDefinition>().ToArray();
                if (methods.Length == 0)
                {
                    methodBuffer.Add("{ }");
                }
                else
                {
                    methodBuffer.Add("{ ");
                    for (int i = 0; i < methods.Length; i++)
                    {
                        if (i > 0) methodBuffer.Add(", ");
                        methodBuffer.Add("f_" + methods[i].Name);
                        methodBuffer.Add(": ");
                        methodBuffer.Add("mt_" + cd.Name + "_");
                        methodBuffer.Add(methods[i].Name);
                    }
                    methodBuffer.Add(" }");
                }
                methodBuffer.Add(" };");
                methodBuffer.Add(this.NL);
            }
            outputFilePieces.Add(string.Join("", methodBuffer));

            outputFilePieces.Add(string.Join('\n', new string[] {
                "return { ",
                string.Join('\n', pubFuncs.Select(pb => pb.Name).Select(funcName => {
                    return "  " + funcName + ": function() { return vutilUnwrapNative(" + funcName + "(...[...arguments].map(vutilWrapNative))); },";
                })),
                "};",
                "})();",
            }));

            System.IO.File.WriteAllText(
                System.IO.Path.Combine(destDir, "gen.js"),
                string.Join("\n\n", outputFilePieces));
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

        protected override void SerializeArrayIndex(ArrayIndex arrIndex, bool useWrap)
        {
            SerializeExpression(arrIndex.Root, false);
            Append('[');
            SerializeExpression(arrIndex.Index, false);
            Append(']');

            if (!useWrap)
            {
                Append(".value");
            }
        }

        protected override void SerializeAssignmentToField(FieldReference target, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();

            SerializeExpression(target.InstanceContext, false);
            Append(".f_" + target.FieldDefinition.Name);
            Append(" = ");
            if (floatCast)
            {
                Append("vutilGetFloat(");
                SerializeExpression(asgn.Value, false);
                Append(')');
            }
            else
            {
                SerializeExpression(asgn.Value, true);
            }

            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeAssignmentToMap(MapAccess ma, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();
            if (op != "=") throw new System.NotImplementedException(); // TODO: temporary storage of root expression and key computation if not a direct variable or constants.
            Expression key = ma.Key;
            Append("vutilMapSet(");
            SerializeExpression(ma.Root, true);
            Append(", ");
            SerializeExpression(key, true);
            Append(", ");
            SerializeExpression(asgn.Value, true);
            Append(")");

            if (!omitSemicolon) ApplyExecSuffix();
        }

        protected override void SerializeAssignmentToVariable(Variable v, Assignment asgn, bool omitSemicolon, bool floatCast)
        {
            string op = asgn.Op.Value;
            if (!omitSemicolon) ApplyExecPrefix();
            Append(v.Name);
            Append(" = ");

            if (op == "=")
            {
                if (floatCast)
                {
                    Append("vutilGetFloat(");
                    SerializeExpression(asgn.Value, false);
                    Append(')');
                }
                else
                {
                    SerializeExpression(asgn.Value, true);
                }
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

        protected override void SerializeConstructor(ConstructorDefinition ctor)
        {
            ClassDefinition cd = ctor.WrapperClass;
            ApplyExecPrefix();
            Append("const ");
            Append("ctor_");
            Append(cd.Name);

            Append(" = (_this");
            for (int i = 0; i < ctor.Args.Length; i++)
            {
                Append(", ");
                Append(ctor.Args[i].Name);
            }
            Append(") => {");
            Append(this.NL);
            this.TabLevel++;

            foreach (Field memberField in cd.Members.OfType<Field>())
            {
                Append(this.CurrentTab);
                Append("_this.value.f_");
                Append(memberField.Name);
                Append(" = ");
                if (memberField.StartingValue != null)
                {
                    SerializeExpression(memberField.StartingValue, true);
                }
                else
                {
                    Type type = memberField.Type;
                    if (type.IsInteger) Append("vctx.constZero");
                    else if (type.IsFloat) Append("vctx.constZeroF");
                    else if (type.IsBoolean) Append("vctx.constFalse");
                    else Append("vctx.constNull");
                }
                Append(';');
                Append(NL);
            }

            SerializeExecutables(ctor.Body);
            Append(this.CurrentTab);
            Append("return _this;");
            Append(this.NL);
            this.TabLevel--;
            Append(this.CurrentTab);
            Append("};");
            Append(this.NL);
            Append(this.NL);
        }

        protected override void SerializeMethodInvocation(MethodInvocation mi, bool useWrap)
        {
            // TODO: check if the root expression is simple like a variable 
            // and just duplicate the instance context twice instead of this
            // lambda nonsense.
            Append("((_t) => _t.value.f_");
            Append(mi.FuncRef.FunctionDefinition.Name);
            Append("(_t");
            for (int i = 0; i < mi.ArgList.Length; i++)
            {
                Append(", ");
                SerializeExpression(mi.ArgList[i], true);
            }
            Append("))(");
            SerializeExpression(mi.InstanceContext, true);
            Append(')');
            if (!useWrap) Append(".value");
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

        protected override void SerializeFieldReference(FieldReference fieldRef, bool useWrap)
        {
            SerializeExpression(fieldRef.InstanceContext, false);
            Append(".f_");
            Append(fieldRef.FieldDefinition.Name);
            if (!useWrap) Append(".value");
        }

        protected override void SerializeFloatCast(FloatCast fc, bool useWrap)
        {
            if (!useWrap)
            {
                SerializeExpression(fc.Expression, false);
            }
            else
            {
                Append("vutilGetFloat(");
                SerializeExpression(fc.Expression, false);
                Append(')');
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

        protected override void SerializeConstructorInvocation(ConstructorInvocation ctorInvoke)
        {
            ClassDefinition cd = ctorInvoke.ResolvedType.ResolvedClass;
            Append("ctor_");
            Append(cd.Name);
            Append("(");
            Append("vutilNewInstance('");
            Append(cd.Name);
            Append("')");

            for (int i = 0; i < ctorInvoke.Args.Length; i++)
            {
                Append(", ");
                SerializeExpression(ctorInvoke.Args[i], true);
            }
            Append(')');
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
            ClassDefinition classDef = fd.WrapperClass;
            ApplyExecPrefix();
            Append("const ");
            if (fd.WrapperClass != null)
            {
                Append("mt_");
                Append(classDef.Name);
                Append('_');
            }
            Append(fd.Name);

            Append(" = (");

            if (classDef != null)
            {
                Append("_this");
                if (fd.Args.Length > 0)
                {
                    Append(", ");
                }
            }
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

        protected override void SerializeGetFieldValue(DotField df, bool useWrap)
        {
            SerializeExpression(df.Root, false);
            Append(".f_");
            Append(df.FieldName);
            if (!useWrap) Append(".value");
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
            int value = ic.Value;

            if (value < 1000 && value > -1000)
            {
                if (useWrap)
                {
                    if (value == 0) Append("vctx.constZero");
                    else if (value == 0) Append("vctx.constOne");
                    else
                    {
                        if (value < 0) Append("vctx.numNeg[");
                        else Append("vctx.numPos[");
                        Append(value + "]");
                    }
                }
                else
                {
                    Append("" + value);
                }
            }
            else
            {
                if (useWrap)
                {
                    Append("vutilGetInt(");
                    Append(value + ")");
                }
                else
                {
                    Append(value + "");
                }
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

        protected override void SerializeNullConstant(bool useWrap)
        {
            Append(useWrap ? "vctx.constNull" : "null");
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

        protected override void SerializeStringComparison(Expression str1, Expression str2, bool useWrap)
        {
            Append('(');
            Append('(');
            SerializeExpression(str1, false);
            Append(") === (");
            SerializeExpression(str2, false);
            Append(')');
            if (useWrap) Append(" ? vctx.constTrue : vctx.constFalse)");
            else Append(')');
        }

        protected override void SerializeStringConcatChain(StringConcatChain strChain, bool useWrap)
        {
            if (useWrap) Append("vutilGetString(");
            else Append('(');

            if (strChain.Expressions.Length == 2)
            {
                Append('(');
                SerializeExpression(strChain.Expressions[0], false);
                Append(") + (");
                SerializeExpression(strChain.Expressions[1], false);
                Append(')');
            }
            else
            {
                Append('[');
                for (int i = 0; i < strChain.Expressions.Length; i++)
                {
                    if (i > 0) Append(", ");
                    SerializeExpression(strChain.Expressions[i], false);
                }
                Append("].join('')");
            }

            Append(')');
        }

        protected override void SerializeStringConstant(StringConstant sc, bool useWrap)
        {
            // TODO: this is a terrible hack
            string codeValue = "`" + sc.Value.Replace("\\", "\\\\").Replace("'", "\\'") + "`";
            if (useWrap)
            {
                if (sc.Value.Length == 0)
                {
                    Append("vctx.emptyString");
                }
                else
                {
                    // TODO: build a string table, like in the C version
                    Append("vutilGetCommonString(");
                    Append(codeValue);
                    Append(')');
                }
            }
            else
            {
                Append(codeValue);
            }
        }

        protected override void SerializeThisConstant(ThisConstant thiz, bool useWrap)
        {
            Append("_this");
            if (!useWrap)
            {
                Append(".value");
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

        protected override void SerializeSysFuncListLength(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutilGetInt(");
            Append('(');
            SerializeExpression(expr, false);
            Append(").length");
            if (useWrap) Append(')');
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
                Append("vutilNewMap(" + (keyType.IsInteger ? "true" : "false") + ")");
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

        protected override void SerializeSysFuncStringReplace(Expression str, Expression needle, Expression newValue, bool useWrap)
        {
            if (useWrap) Append("vutilGetString(");
            Append('(');
            SerializeExpression(str, false);
            Append(").split(");
            SerializeExpression(needle, false);
            Append(").join(");
            SerializeExpression(newValue, false);
            Append(')');
            if (useWrap) Append(')');
        }

        protected override void SerializeSysFuncStringToCharArray(Expression str, bool useWrap)
        {
            // TODO: helper function that takes into consideration surrogate pairs.
            if (useWrap) Append("{ type: 'A', value: ");
            Append("(");
            SerializeExpression(str, false);
            Append(").split('').map(vutilGetCommonString)"); // because they're single characters
            if (useWrap) Append(" }");
        }

        protected override void SerializeSysFuncStringTrim(Expression str, bool useWrap)
        {
            if (useWrap) Append("vutilGetString(");
            Append('(');
            SerializeExpression(str, false);
            Append(").trim()");
            if (useWrap) Append(')');
        }

        protected override void SerializeSysPropListLength(Expression expr, bool useWrap)
        {
            if (useWrap) Append("vutilGetInt(");
            SerializeExpression(expr, false);
            Append(".length");
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
