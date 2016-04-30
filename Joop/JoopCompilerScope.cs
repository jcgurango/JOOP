using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop
{
    public abstract class JoopCompilerScope
    {
        public const string EXPECT_DEFAULT_ERROR = "Unexpected token \"{0}\".";
        public const string REGEX_IDENTIFIER = @"^[a-zA-Z_]\w+$";
        public const string ERROR_BADIDENTIFIER = "Bad identifier \"{0}\".";

        protected JoopCompilerScope ParentScope
        {
            get;
            set;
        }

        protected StringBuilder Output
        {
            get;
            set;
        }

        protected JoopTokenCollection Tokens
        {
            get;
            set;
        }

        public int BlockOffset
        {
            get;
            set;
        }

        protected JoopToken CurrentToken
        {
            get
            {
                return Tokens[tokenIndex];
            }
        }

        private int tokenIndex = 0;

        public JoopCompilerScope()
        {
        }

        protected abstract void WriteOpening();
        protected abstract void Parse();
        protected abstract void WriteClosing();

        protected virtual void ScopeBlock(JoopCompilerScope childScope, JoopToken block)
        {
            childScope.BlockOffset = BlockOffset + block.Index + 1;
            childScope.Parse(this, this.Output, block.Content);
        }

        protected bool NextToken()
        {
            do
            {
                tokenIndex++;

                if (tokenIndex >= Tokens.Count)
                {
                    return false;
                }
            }
            while (SkipCurrentToken());

            return true;
        }

        protected virtual bool SkipCurrentToken()
        {
            return false;
        }

        protected bool Expect(string content, string message = EXPECT_DEFAULT_ERROR)
        {
            string errorMessage = string.Format(message,
                content);

            return Expect(token => token.Content == content, errorMessage);
        }

        protected bool Expect(JoopTokenType type)
        {
            string errorMessage = string.Format("Unexpected token of type {0}. Expecting {1}.",
                CurrentToken.Type,
                type);

            return Expect(token => token.Type == type, errorMessage);
        }

        protected bool Expect(Func<JoopToken, bool> acceptable, string message = EXPECT_DEFAULT_ERROR)
        {
            if (Accept(acceptable))
            {
                return true;
            }

            throw CompilerError(string.Format(message, CurrentToken.Content),  CurrentToken.Index);
        }

        protected bool AcceptIdentifier()
        {
            return Accept(
                    token => IsIdentifier(token.Content)
                );
        }

        protected bool ExpectIdentifier()
        {
            return Expect(
                    token => IsIdentifier(token.Content),
                    CreateErrorMessage(ERROR_BADIDENTIFIER)
                );
        }

        protected bool IsIdentifier(string identifier)
        {
            return Regex.IsMatch(identifier, REGEX_IDENTIFIER);
        }

        protected string CreateErrorMessage(string error)
        {
            return string.Format(error, CurrentToken.Content);
        }

        protected bool Accept(string content)
        {
            return Accept(token => token.Content == content);
        }

        protected bool Accept(JoopTokenType type)
        {
            return Accept(token => token.Type == type);
        }

        protected bool Accept(Func<JoopToken, bool> acceptable)
        {
            if (acceptable(CurrentToken))
            {
                NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected JoopCompilerException CompilerError(string errorMessage, int tokenPosition)
        {
            return new JoopCompilerException(errorMessage, BlockOffset + tokenPosition);
        }

        public void Parse(JoopCompilerScope parentScope, StringBuilder output, string block)
        {
            ParentScope = parentScope;
            Output = output;

            Tokens = JoopTokenizer.Tokenize(block);
            tokenIndex = 0;
            WriteOpening();

            for (int i = 0; i < 1000 && tokenIndex < Tokens.Count; i++)
            {
                Parse();
            }

            WriteClosing();
        }
    }
}
