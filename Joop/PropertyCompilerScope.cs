using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joop
{
    public class PropertyCompilerScope : ClassCompilerScope
    {
        public string PropertyName
        {
            get;
            set;
        }

        public bool IsStatic
        {
            get;
            set;
        }

        public const string TOKEN_GET = "get";
        public const string TOKEN_SET = "set";

        public PropertyCompilerScope(string className, string baseClassName, string propertyName, bool isStatic)
            : base(className, baseClassName)
        {
            PropertyName = propertyName;
            IsStatic = isStatic;
        }

        protected override void WriteOpening()
        {
            // This scope only parses properties, so we're overriding the entire opening process of
            // the class scope.

        }

        protected override void Parse()
        {
            // This scope only parses properties, so we're overriding the entire parsing process of
            // the class scope.
            if (Accept(TOKEN_GET))
            {
                ParseFunctionDefinition(string.Format("get_{0}", PropertyName), IsStatic);
                Output.AppendLine(string.Format("{0}.get_{1} = get_{1};", ClassName + (!IsStatic ? ".prototype" : ""), PropertyName));

            }
            else if (Expect(TOKEN_SET, "Expecting get or set definition of property."))
            {
                ParseFunctionDefinition(string.Format("set_{0}", PropertyName), IsStatic, "value");
                Output.AppendLine(string.Format("{0}.set_{1} = set_{1};", ClassName + (!IsStatic ? ".prototype" : ""), PropertyName));
            }
        }

        protected override void WriteClosing()
        {
            // This scope only parses properties, so we're overriding the closing opening process of
            // the class scope.


        }
    }
}
