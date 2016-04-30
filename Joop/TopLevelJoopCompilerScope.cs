using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public class TopLevelJoopCompilerScope : NamespaceCompilerScope
    {
        private const string FUNCTION_NAME_USING = "$using";
        private const string VARIABLE_NAME_GLOBAL = "$global";
        private const string FUNCTION_NAME_EXTEND = "$extend";
        private const string FUNCTION_NAME_INHERITS = "$inherits";
        public const string TOKEN_PROG = "prog";

        public TopLevelJoopCompilerScope()
            : base(null)
        {
        }

        protected override void WriteOpening()
        {
            // The very top level comment will appear before any other comments as long as
            // it is a block-type comment written with 4 or more asterisks.
            if (CurrentToken.Type == JoopTokenType.Comment && CurrentToken.Content.StartsWith("/****"))
            {
                Output.AppendLine(CurrentToken.Content);
                NextToken();
            }

            Output.AppendFormat("var {0} = {0} || this;", VARIABLE_NAME_GLOBAL);
            Output.AppendFormat("var {0} = {0}", FUNCTION_NAME_EXTEND);
            Output.AppendLine(" || (function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; function __() { this.constructor = d; } __.prototype = b.prototype; d.prototype = new __(); });");
            Output.AppendFormat("var {0} = {0}", FUNCTION_NAME_USING);
            Output.AppendLine(" || (function (n, x) { for (var i in n) { x[i] = n[i]; } });");
            Output.AppendFormat("var {0} = {0}", FUNCTION_NAME_INHERITS);
            Output.AppendLine(" || (function(other) { var otherClass; if (typeof other == \"string\") { otherClass = other; } else { otherClass = other.getQualifiedType(); } return (this.getBaseTypes().indexOf(\"|\" + otherClass + \"|\") > -1); });");
            Output.AppendLine();

            base.WriteOpening();
        }

        protected override void Parse()
        {
            // At the top level, the "prog" keyword can be used.
            if (Accept(TOKEN_PROG))
            {
                string content = CurrentToken.Content;
                Expect(JoopTokenType.Block);
                Output.AppendLine(content);
            }
            else
            {
                base.Parse();
            }
        }
    }
}
