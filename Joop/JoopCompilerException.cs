using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joop
{
    public class JoopCompilerException : Exception
    {
        public int Index
        {
            get;
            set;
        }

        public JoopCompilerException(string message, int index)
            : base(message)
        {
            this.Index = index;
        }
    }
}
