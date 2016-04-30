using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    /// <summary>
    ///     Splits a string into comment, block, or token.
    /// </summary>
    public class JoopTokenizer
    {
        public const string REGEX_JS_STRING = @"((['""])(?:(?!\2|\\).|\\(?:\r\n|[\s\S]))*(\2)?|`(?:[^`\\$]|\\[\s\S]|\$(?!\{)|\$\{(?:[^{}]|\{[^}]*\}?)*\}?)*(`)?)";
        public const string REGEX_COMMENT_LINE = @"\/\/[\s\S]+?\r?\n";
        public const string REGEX_COMMENT_BLOCK = @"\/\*[\s\S]+?\*\/";
        public const string REGEX_WORD = @"\w+";
        public const string REGEX_ANY = @"\S";
        public const string REGEX_WHITESPACE = @"\s+";
        public const string TOKEN_LBRACKET = "{";
        public const string TOKEN_RBRACKET = "}";

        // Group 1 - Comment.
        // Group 2 - Token.
        // Group 3 - Whitespace.
        private static readonly string REGEX_ALL = string.Format(
            @"{0}|({1}|{2})|({3}|{4})|({5})",
            REGEX_JS_STRING,
            REGEX_COMMENT_LINE,
            REGEX_COMMENT_BLOCK,
            REGEX_WORD,
            REGEX_ANY,
            REGEX_WHITESPACE
        );

        public static JoopTokenCollection Tokenize(string text)
        {
            JoopTokenCollection collection = new JoopTokenCollection();
            Match match = Regex.Match(text, REGEX_ALL);

            do
            {
                Read(collection, ref match);
                match = match.NextMatch();
            }
            while (match.Success);

            return collection;
        }

        private static void Read(JoopTokenCollection collection, ref Match match)
        {
            if (match.Groups[5].Success)
            {
                // Comment.
                collection.Add(new JoopToken(JoopTokenType.Comment, match.Groups[5].Value, match.Index));
            }
            else if (match.Groups[6].Success || match.Groups[1].Success)
            {
                // Token.
                if (match.Groups[6].Value == TOKEN_LBRACKET)
                {
                    // Block.
                    ReadBlock(collection, ref match);
                }
                else
                {
                    // Just regular token.
                    collection.Add(new JoopToken(JoopTokenType.Token, match.Groups[6].Value, match.Index));
                }
            }
            else if (match.Groups[7].Success)
            {
                // Whitespace; do nothing, but also don't throw any exception.
            }
            else
            {
                throw new JoopCompilerException(string.Format("Unrecognized token \"{0}\".", match.Value), match.Index);
            }
        }

        private static void ReadBlock(JoopTokenCollection collection, ref Match match)
        {
            StringBuilder blockValue = new StringBuilder();
            int blockIndex = match.Index;
            Match lastMatch = null;
            int counter = 1;

            while (counter > 0 && match.Success)
            {
                lastMatch = match;
                match = match.NextMatch();

                if (match.Value == TOKEN_LBRACKET)
                {
                    counter++;
                }

                if (match.Value == TOKEN_RBRACKET)
                {
                    counter--;
                }

                if (counter > 0)
                {
                    blockValue.Append(match.Value);
                }
            }

            if (counter > 0)
            {
                throw new JoopCompilerException("Unexpected end of file.", (lastMatch == null ? 0 : lastMatch.Index + lastMatch.Length));
            }
            else
            {
                collection.Add(new JoopToken(JoopTokenType.Block, blockValue.ToString(), blockIndex));
            }
        }
    }
}
