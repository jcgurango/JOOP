using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public class MethodReplacer
    {
        public class Replacement
        {
            public string Regex
            {
                get;
                set;
            }

            public MatchEvaluator Evaluator
            {
                get;
                set;
            }

            public string Text
            {
                get;
                set;
            }
        }

        public string BaseClassName
        {
            get;
            set;
        }

        public Replacement[] Replacements;

        public MethodReplacer(string baseClassName)
        {
            BaseClassName = baseClassName;

            Replacements = new Replacement[]
            {
                new Replacement()
                {
                    Regex = @"(?<!\w)base[\s]*\((\s*\))?",
                    Evaluator = new MatchEvaluator(
                            (match) =>
                            {
                                if (match.Groups[1].Value.Length > 0)
                                {
                                    return string.Format("{0}.call(this)", BaseClassName);
                                }

                                return string.Format("{0}.call(this, ", BaseClassName);
                            }
                        )
                },
                new Replacement()
                {
                    Regex = @"(?<!\w)base\s*\.\s*(\w+)\s*\((\s*\))?",
                    Evaluator = new MatchEvaluator(
                        (match) =>
                        {
                            if (match.Groups[2].Value.Length > 0)
                            {
                                return string.Format("{0}.prototype.{1}.call(this)", BaseClassName, match.Groups[1].Value);
                            }

                            return string.Format("{0}.prototype.{1}.call(this, ", BaseClassName, match.Groups[1].Value);
                        }
                    )
                },
                new Replacement()
                {
                    Regex = @"(?<!\w)this.private[\s]*\.",
                    Text = "privObj[this.privKey]."
                }
            };
        }

        public string Replace(string block)
        {
            string current = block;

            foreach (Replacement replacement in Replacements)
            {
                current = InternalReplace(current, replacement);
            }

            return current;
        }

        protected virtual string InternalReplace(string current, Replacement replacement)
        {
            if (replacement.Evaluator != null)
            {
                return Regex.Replace(current, replacement.Regex, replacement.Evaluator);
            }
            else if (!string.IsNullOrEmpty(replacement.Text))
            {
                return Regex.Replace(current, replacement.Regex, replacement.Text);
            }

            return current;
        }
    }
}
