using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public class ClassCompilerScope : JoopCompilerScope
    {
        public const string TOKEN_LPAREN = "(";
        public const string TOKEN_RPAREN = ")";
        public const string TOKEN_COMMA = ",";
        public const string TOKEN_SEMICOLON = ";";

        public string ClassName
        {
            get;
            protected set;
        }

        public string BaseClassName
        {
            get;
            protected set;
        }

        private bool initialized = false;

        public ClassCompilerScope(string className, string baseClassName)
        {
            ClassName = className;
            BaseClassName = baseClassName;
        }

        protected override void WriteOpening()
        {
            Output.Append("// Class " + ClassName);

            if (!string.IsNullOrEmpty(BaseClassName))
            {
                Output.AppendLine(" extends " + BaseClassName);
            }

            Output.AppendLine();
        }

        protected override void Parse()
        {
            bool isStatic = false;

            if (Accept("static"))
            {
                isStatic = true;
            }

            int defPosition = CurrentToken.Index;

            if (Accept("function"))
            {
                if (!initialized)
                {
                    WriteDefaultConstructor();
                    initialized = true;
                }

                string functionName = CurrentToken.Content;

                if (ExpectIdentifier())
                {
                    Output.AppendLine("// Function " + functionName);
                    string[] arguments = ReadArguments();
                    ParseFunctionDefinition(functionName, isStatic, arguments);
                    Output.AppendLine(ClassName + "." + (!isStatic ? "prototype." : "") + functionName + " = " + functionName + ";");
                }
            }
            else if (Accept("constructor"))
            {
                if (isStatic)
                {
                    throw CompilerError("Constructors cannot be marked static. Consider using prog instead.", defPosition);
                }

                if (initialized)
                {
                    throw CompilerError("Constructors must be defined before any properties or functions.", defPosition);
                }

                ParseConstructorDefinition();
            }
            else if (Expect("prop", "Expecting function, property, or constructor definition."))
            {
                if (!initialized)
                {
                    WriteDefaultConstructor();
                    initialized = true;
                }

                ParsePropertyDefinition(isStatic);
            }
        }

        private void ParseConstructorDefinition()
        {
            initialized = true;
            string[] arguments = ReadArguments();

            ParseConstructorBody(arguments);

            if (!string.IsNullOrEmpty(BaseClassName))
            {
                Output.AppendFormat("$extend(" + ClassName + ", " + BaseClassName + ");");
            }

            Output.AppendLine();
            Output.AppendLine("(function()");
            Output.AppendLine("{");
        }

        protected void ParseConstructorBody(string[] arguments)
        {
            string args = string.Join(TOKEN_COMMA, arguments);
            Output.AppendLine(string.Format("function {0}({1})", ClassName, args));

            string defBlock = CurrentToken.Content;

            if (Expect(JoopTokenType.Block))
            {
                // Keep the current token, assuming it's a block.
                Output.AppendLine(JoopTokenizer.TOKEN_LBRACKET);
                Output.AppendLine("this.privKey = privObj.push(new Object()) - 1;");
                Output.AppendLine(TransformFunctionBlock(defBlock));
                Output.AppendLine(JoopTokenizer.TOKEN_RBRACKET);
            }
        }

        protected void ParseFunctionDefinition(string functionName, bool isStatic, params string[] arguments)
        {
            string args = string.Join(TOKEN_COMMA, arguments);
            Output.AppendLine(string.Format("function {0}({1})", functionName, args));

            // Parse the body.
            ParseFunctionBody();
        }

        protected void WriteDefaultConstructor()
        {
            Output.AppendFormat("function {0}()", ClassName);
            Output.AppendLine("{");
            Output.AppendLine("this.privKey = privObj.push(new Object()) - 1;");

            if (!string.IsNullOrEmpty(BaseClassName))
            {
                Output.AppendFormat("{0}.apply(this, arguments);", BaseClassName);
            }

            Output.AppendLine("}");

            if (!string.IsNullOrEmpty(BaseClassName))
            {
                Output.AppendFormat("$extend(" + ClassName + ", " + BaseClassName + ");");
            }

            Output.AppendLine();
            Output.AppendLine("(function()");
            Output.AppendLine("{");
        }

        protected void ParseFunctionBody()
        {
            // Keep the current token, assuming it's a block.
            string defBlock = CurrentToken.Content;

            // Read the function. The block definition can basically be taken verbatim,
            // however there are a couple of replacements.
            // #1 - base.FunctionCall() -> [baseClassName].protoype.FunctionCall.call(this, args);
            // #2 - (In constructors) base(arg0, arg1) -> [baseClassName].call(this, arg0, arg1);

            // The next token must be a block defining the function.
            if (Expect(JoopTokenType.Block))
            {
                Output.Append(JoopTokenizer.TOKEN_LBRACKET);
                Output.AppendLine(TransformFunctionBlock(defBlock));
                Output.AppendLine(JoopTokenizer.TOKEN_RBRACKET);
            }
        }

        protected void ParsePropertyDefinition(bool isStatic)
        {
            if (!initialized)
            {
                WriteDefaultConstructor();
                initialized = true;
            }

            string propertyName = CurrentToken.Content;

            if (ExpectIdentifier())
            {
                Output.AppendLine("// Property " + propertyName);
                JoopToken block = CurrentToken;

                if (Accept(JoopTokenType.Block))
                {
                    // Defined property. A little more complicated.
                    PropertyCompilerScope propertyScope = new PropertyCompilerScope(ClassName, BaseClassName, propertyName, isStatic);
                    ScopeBlock(propertyScope, block);
                }
                else if (Expect(TOKEN_SEMICOLON))
                {
                    // Automatic property.
                    Output.AppendLine(string.Format(
                        "function get_{0}() {{ return this.$prop_{0}; }}\r\n" +
                        "{1}.get_{0} = get_{0};\r\n" +
                        "function set_{0}(value) {{ this.$prop_{0} = value; }}\r\n" +
                        "{1}.set_{0} = set_{0};\r\n", propertyName, ClassName + (!isStatic ? ".prototype" : "")));
                }
            }
        }

        private string[] ReadArguments()
        {
            List<string> argumentNames = new List<string>();

            if (Expect(TOKEN_LPAREN))
            {
                string lastIdentifier = CurrentToken.Content;

                while (AcceptIdentifier())
                {
                    argumentNames.Add(lastIdentifier);

                    if (Accept(TOKEN_COMMA))
                    {
                        lastIdentifier = CurrentToken.Content;
                    }
                    else
                    {
                        break;
                    }
                }

                Expect(TOKEN_RPAREN, "Incomplete function declaration.");
            }

            return argumentNames.ToArray();
        }

        protected virtual string TransformFunctionBlock(string block)
        {
            MethodReplacer replacer = new MethodReplacer(BaseClassName);

            return replacer.Replace(block.Trim());
        }

        protected override void WriteClosing()
        {
            Output.AppendLine("})();");

            if (!initialized)
            {
                WriteDefaultConstructor();
                initialized = true;
            }

            // Add the class to the parent namespace.
            if (ParentScope != null)
            {
                Output.Append(GetParentName());
                Output.AppendLine("." + ClassName + " = " + ClassName + ";");
            }
        }

        protected string GetParentName()
        {
            NamespaceCompilerScope parent = (ParentScope as NamespaceCompilerScope);

            if (parent != null && !string.IsNullOrEmpty(parent.NamespaceName))
            {
                return parent.NamespaceName;
            }

            return "$global";
        }
    }
}
