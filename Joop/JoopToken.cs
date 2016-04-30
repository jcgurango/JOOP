using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Joop
{
    public struct JoopToken
    {
        public JoopTokenType Type
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public JoopToken(JoopTokenType tokenType, string content, int index)
            : this()
        {
            Type = tokenType;
            Content = content;
            Index = index;
        }
    }

    public enum JoopTokenType
    {
        Comment,
        Block,
        Token
    }

    public class JoopTokenCollection : Collection<JoopToken> { }
}
