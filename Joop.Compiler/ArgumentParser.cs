using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Joop.Compiler
{
    public class ArgumentParser
    {
        private class StringArrayDescender : IDescender
        {
            int currentToken = 0;
            string[] tokens;

            public string CurrentToken
            {
                get
                {
                    if (currentToken < tokens.Length)
                    {
                        return tokens[currentToken];
                    }

                    return null;
                }
            }

            public bool Accept(params string[] tokens)
            {
                if (Array.IndexOf(tokens, (CurrentToken ?? "").ToLower()) > -1)
                {
                    next();
                    return true;
                }

                return false;
            }

            public string Keep()
            {
                string token = CurrentToken;
                next();
                return token;
            }

            private void next()
            {
                currentToken++;
            }

            public bool MoreTokens()
            {
                return currentToken < tokens.Length;
            }

            public StringArrayDescender(string[] tokens)
            {
                this.tokens = tokens;
            }
        }

        public IDescender Descender
        {
            get;
            set;
        }

        public ParsingOutput Output
        {
            get;
            set;
        }

        public ArgumentParser()
        {
        }

        public ArgumentParser(IDescender descender)
        {
            this.Descender = descender;
        }

        public void Parse(string[] args)
        {
            this.Descender = new StringArrayDescender(args);
            Parse();
        }

        public void Parse()
        {
            Output = new ParsingOutput();
            MSBuildErrors();

            if (Descender.Accept("-d", "--dirmode"))
            {
                FolderMode(); 
            }
            else if (Descender.Accept("-f", "--filemode"))
            {
                FileMode();
            }
            else
            {
                Output.PrintHelp = true;
            }
        }

        private void FolderMode()
        {
            bool rootOnly = Descender.Accept("-r", "--rootonly");
            SearchOption option = GetSearchOption(rootOnly);
            
            if (Descender.Accept("-i", "--indir"))
            {
                string inputDirectory = Descender.Keep();

                if (inputDirectory == null)
                {
                    throw new Exception("Invalid input directory.");
                }

                // Read the input directory.
                string[] files = Directory.GetFiles(inputDirectory, "*.joop", option);

                foreach (string file in files)
                {
                    Output.Inputs.Add(file);
                }

                // Figure out the output type.
                OutputType();
            }
            else
            {
                throw new Exception("The --indir parameter is required for directory mode.");
            }
        }

        private void OutputType()
        {
            if (Descender.Accept("-s", "--singlefile"))
            {
                SingleFile();
            }
            else if (Descender.Accept("-a", "--autoname"))
            {
                // Automatically name all the files.
                foreach (string file in Output.Inputs)
                {
                    string autoName = AutoName(file);

                    if (autoName == file)
                    {
                        throw new Exception("The input file \"" + file + "\" is invalid as it would have the same auto-name.");
                    }

                    Output.Outputs.Add(autoName);
                }
            }
            else
            {
                throw new Exception("You must specify either the --autoname or --singlefile option.");
            }

            ExpectEnd();
        }

        private void SingleFile()
        {
            string outFile = Descender.Keep();

            if (outFile == null)
            {
                throw new Exception("Invalid output file.");
            }

            Output.Outputs.Add(outFile);
            Output.SingleFile = true;
        }

        private void ExpectEnd()
        {
            if (Descender.MoreTokens())
            {
                throw new Exception("Incorrect arguments.");
            }
        }

        private void MSBuildErrors()
        {
            Output.MSBuildErrors = Descender.Accept("-m", "--msbuild");
        }

        private void FileMode()
        {
            bool autoName = Descender.Accept("-a", "--autoname");

            if (Descender.Accept("-s", "--singlefile"))
            {
                SingleFile();
            }
            else if (Descender.Accept("-a", "--autoname"))
            {
                autoName = true;
            }

            while (Descender.Accept("-i", "--input"))
            {
                string inFile = Descender.Keep();

                if (inFile == null)
                {
                    throw new Exception("Invalid input file.");
                }

                if (!File.Exists(inFile))
                {
                    throw new Exception("File \"" + inFile + "\" does not exist.");
                }

                Output.Inputs.Add(inFile);

                if (Descender.Accept("-o", "--output"))
                {
                    string outFile = Descender.Keep();

                    if (outFile == null)
                    {
                        throw new Exception("Invalid output file.");
                    }

                    Output.Outputs.Add(outFile);
                }
                else
                {
                    if (autoName)
                    {
                        Output.Outputs.Add(AutoName(inFile));
                    }
                    else
                    {
                        throw new Exception("All --input directives must be followed by a --output directive if the --autoname option is not set.");
                    }
                }

                if (Descender.CurrentToken != "-i" && Descender.CurrentToken != "--input" && Descender.CurrentToken != null)
                {
                    throw new Exception("Incorrect directive: " + Descender.CurrentToken);
                }
            }

            ExpectEnd();
        }

        private static string AutoName(string file)
        {
            string autoName = Path.Combine(
                Path.GetDirectoryName(file),
                Path.GetFileNameWithoutExtension(file) + ".js"
            );
            return autoName;
        }

        private static SearchOption GetSearchOption(bool rootOnly)
        {
            SearchOption option;

            if (rootOnly)
            {
                option = SearchOption.TopDirectoryOnly;
            }
            else
            {
                option = SearchOption.AllDirectories;
            }
            return option;
        }
    }

    public interface IDescender
    {
        string CurrentToken
        {
            get;
        }

        bool Accept(params string[] tokens);
        string Keep();
        bool MoreTokens();
    }

    public class ParsingOutput
    {
        public List<string> Inputs
        {
            get;
            set;
        }

        public List<string> Outputs
        {
            get;
            set;
        }

        public bool SingleFile
        {
            get;
            set;
        }

        public bool MSBuildErrors
        {
            get;
            set;
        }

        public bool PrintHelp
        {
            get;
            set;
        }

        public ParsingOutput()
        {
            Inputs = new List<string>();
            Outputs = new List<string>();
        }
    }
}
