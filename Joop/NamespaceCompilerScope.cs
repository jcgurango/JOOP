using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public class NamespaceCompilerScope : JoopCompilerScope
    {
        public const string TOKEN_NAMESPACE = "namespace";
        public const string TOKEN_CLASS = "class";
        public const string TOKEN_NAMESPACE_SPLITTER = ".";
        public const string TOKEN_INHERITCLASS = ":";

        public string NamespaceName
        {
            get;
            protected set;
        }

        public NamespaceCompilerScope(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        protected override void WriteOpening()
        {
            if (!string.IsNullOrEmpty(NamespaceName))
            {
                // Split the namespace into its hierarchy.
                string[] splitNamespace = NamespaceName.Split(new string[] { TOKEN_NAMESPACE_SPLITTER }, StringSplitOptions.None);

                // Initialize each level of the hierarchy individually.
                for (int level = 1; level <= splitNamespace.Length; level++)
                {
                    if (level == 1)
                    {
                        Output.AppendLine("var " + splitNamespace[0] + " = " + splitNamespace[0] + "||new Object();");
                    }
                    else
                    {
                        for (int i = 0; i < level; i++)
                        {
                            if (i > 0)
                            {
                                Output.Append(".");
                            }

                            Output.Append(splitNamespace[i]);
                        }

                        Output.Append("=");

                        for (int i = 0; i < level; i++)
                        {
                            if (i > 0)
                            {
                                Output.Append(".");
                            }

                            Output.Append(splitNamespace[i]);
                        }

                        Output.Append("||");
                        Output.Append("new Object();");
                        Output.AppendLine();
                    }
                }

                // Write a comment for the name of the namespace.
                Output.AppendLine("// Namespace " + NamespaceName);
            }

            Output.AppendLine("(function()");
            Output.AppendLine("{");
            Output.AppendLine("var privObj = [];");
        }

        protected override void Parse()
        {
            // Initially there must be either a comment, namespace, or class declaration. Other
            // declarations should result in an error.
            if (CurrentToken.Type == JoopTokenType.Comment)
            {
                // Comments just need to be appended normally. They shouldn't have any bearing on
                // execution.
                Output.AppendLine(CurrentToken.Content);
                NextToken();
            }
            else if (Accept(TOKEN_NAMESPACE))
            {
                // Parse the definition of the namespace.
                ParseNamespaceDefinition();
            }
            else if (Expect(TOKEN_CLASS, "Expecting namespace or class declaration."))
            {
                // Parse the definition of the class.
                ParseClassDefinition();
            }

            // Append a line to separate the output.
            Output.AppendLine();
        }

        private void ParseNamespaceDefinition()
        {
            string namespaceName = ReadFullyQualifiedName();

            // The next token must be a full code block describing the sub-namespace.
            JoopToken namespaceBlockToken = CurrentToken;

            if (Expect(JoopTokenType.Block))
            {
                NamespaceCompilerScope compilerScope = new NamespaceCompilerScope(namespaceName);
                ScopeBlock(compilerScope, namespaceBlockToken);
            }
        }

        protected string ReadFullyQualifiedName()
        {
            // Keep a list for the fully qualified name.
            List<string> fullNamespace = new List<string>();

            // full names are in identifier.identifier.identifier.identifier format. This must
            // be parsed individually.
            do
            {
                fullNamespace.Add(CurrentToken.Content);
            }
            while (ExpectIdentifier() && Accept(TOKEN_NAMESPACE_SPLITTER));

            return String.Join(TOKEN_NAMESPACE_SPLITTER, fullNamespace);
        }

        private void ParseClassDefinition()
        {
            // Tentatively keep the current token.
            string className = CurrentToken.Content;

            // The next token is the entire identifier of the class, and must match the
            // identifier regex.
            if (ExpectIdentifier())
            {
                string baseClassName = null;

                // Optionally, we can also have a base class.
                if (Accept(TOKEN_INHERITCLASS))
                {
                    baseClassName = ReadFullyQualifiedName();
                }

                // The next token should be an entire code block defining the class.
                JoopToken classBlockToken = CurrentToken;
                
                if(Expect(JoopTokenType.Block))
                {
                    ClassCompilerScope compilerScope = new ClassCompilerScope(className, baseClassName);
                    ScopeBlock(compilerScope, classBlockToken);
                }
            }
        }

        protected override void WriteClosing()
        {
            if (!string.IsNullOrEmpty(NamespaceName))
            {
                // If there's a parent scope, use that namespace name. Otherwise, use the global
                // namespace.
                string parentNamespaceName = null;
                if (ParentScope != null)
                {
                    parentNamespaceName = (ParentScope as NamespaceCompilerScope).NamespaceName;
                }

                if (!string.IsNullOrEmpty(parentNamespaceName))
                {
                    Output.Append(parentNamespaceName);
                }
                else
                {
                    Output.Append("$global");
                }

                Output.Append(".");
                Output.Append(NamespaceName);
                Output.Append("=");
                Output.Append(NamespaceName);
                Output.Append(";");
            }

            Output.AppendLine();
            Output.AppendLine("})();");
        }
    }
}
