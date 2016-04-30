using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joop.Compiler
{
    public class CompilerOptions
    {
        public bool AutoName
        {
            get;
            set;
        }

        public bool DirectoryMode
        {
            get;
            set;
        }

        public bool RootDirectoryOnly
        {
            get;
            set;
        }

        public bool SingleFileOutputMode
        {
            get;
            set;
        }

        public string InputDirectoryPath
        {
            get;
            set;
        }

        public string OutputFile
        {
            get;
            set;
        }

        public List <string> Inputs
        {
            get;
            set;
        }

        public bool MsBuildErrors
        {
            get;
            set;
        }
    }
}
