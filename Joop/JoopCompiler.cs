using JSBeautifyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public class JoopCompiler
    {
        public static string Compile(string source)
        {
            TopLevelJoopCompilerScope compiler = new TopLevelJoopCompilerScope();
            StringBuilder builder = new StringBuilder();

            compiler.Parse(null, builder, source);

            JSBeautifyOptions options = new JSBeautifyOptions();
            options.preserve_newlines = true;
            options.indent_char = '\t';
            options.indent_size = 1;
            JSBeautify beautify = new JSBeautify(builder.ToString(), options);
            return beautify.GetResult();
        }

        public static void GetNearTokenFromException(string sourceFile, JoopCompilerException e, out string nearToken, out int lineNumber, out int colNumber)
        {
            string sub = sourceFile.Substring(e.Index, Math.Min(100, sourceFile.Length - e.Index));
            Match m = Regex.Match(sub, @"^(\w+)(?:\s|$)");
            nearToken = m.Groups[1].Value;

            sub = sourceFile.Substring(0, Math.Min(e.Index + nearToken.Length, sourceFile.Length));
            MatchCollection matches = Regex.Matches(sub, @"(?:\r?\n|^)(.*)");
            lineNumber = matches.Count;
            colNumber = matches[matches.Count - 1].Groups[1].Value.Length - nearToken.Length + 1;
        }
    }
}
