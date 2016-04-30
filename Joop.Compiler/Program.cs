using GSC.Joop.Compiler.Properties;
using JSBeautifyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Joop.Compiler
{
    class Program
    {
        static bool MSBuildErrors = false;

        static void Main(string[] args)
        {
            ArgumentParser arguments = new ArgumentParser();

            try
            {
                arguments.Parse(args);
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return;
            }

            MSBuildErrors = arguments.Output.MSBuildErrors;

            if (arguments.Output.PrintHelp)
            {
                DisplayHelp();
            }
            else
            {
                if (arguments.Output.SingleFile)
                {
                    BatchCompileSingleFileOutput(arguments.Output.Outputs[0], arguments.Output.Inputs);
                }
                else
                {
                    BatchCompile(arguments.Output.Inputs, arguments.Output.Outputs);
                }

                Console.WriteLine("Compilation successful.");
            }
        }

        private static void BatchCompile(IList<string> inputFiles, IList<string> outputFiles)
        {
            for (int i = 0; i < inputFiles.Count; i++)
            {
                string compiled;

                if (!Compile(inputFiles[i], out compiled))
                {
                    Environment.Exit(100);
                }
                else
                {
                    File.WriteAllText(outputFiles[i], compiled);
                }
            }
        }

        private static void BatchCompileSingleFileOutput(string output, IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                string compiled;

                if (!Compile(file, out compiled))
                {
                    Environment.Exit(100);
                }
                else
                {
                    using (StreamWriter writer = File.AppendText(output))
                    {
                        writer.WriteLine(compiled);
                        writer.WriteLine();
                    }
                }
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(Resources.HelpText);
        }

        //private static void ReadArgs(CompilerOptions options, List<string> inputs, List<string> outputs, out bool valid, out string errorMessage)
        //{
        //    valid = true;
        //    errorMessage = null;

        //    if (options.DirectoryMode)
        //    {
        //        if (!string.IsNullOrEmpty(options.InputDirectoryPath))
        //        {
        //            inputs.Add(options.InputDirectoryPath);
        //        }
        //        else
        //        {
        //            errorMessage = "The --indir parameter must be defined for directory mode.";
        //            valid = false;
        //        }

        //        if (options.Inputs != null && options.Inputs.Count > 0)
        //        {
        //            errorMessage = "The --inoutfiles parameter is not applicable for directory mode.";
        //            valid = false;
        //        }

        //        if (!options.AutoName && !options.SingleFileOutputMode)
        //        {
        //            errorMessage = "You must specify either --singlefile or --autname when using directory mode.";
        //            valid = false;
        //        }
        //    }

        //    if (options.SingleFileOutputMode)
        //    {
        //        if (!string.IsNullOrEmpty(options.OutputFile))
        //        {
        //            outputs.Add(options.OutputFile);
        //        }
        //        else
        //        {
        //            errorMessage = "The --outfile parameter must be defined when using single file mode.";
        //            valid = false;
        //        }
        //    }

        //    if (options.Inputs != null && options.Inputs.Count > 0)
        //    {
        //        foreach (string file in options.Inputs)
        //        {
        //            string[] data = file.Split('|');
        //            string path = data[0];

        //            ValidatePath(ref valid, ref errorMessage, path);
        //            if (!valid) return;

        //            string target = null;

        //            if (data.Length != 2 && !options.SingleFileOutputMode)
        //            {
        //                if (options.AutoName)
        //                {
        //                    target = Path.Combine(
        //                        Path.GetDirectoryName(path),
        //                        Path.GetFileNameWithoutExtension(path) +
        //                        ".js"
        //                    );
        //                }
        //                else
        //                {
        //                    errorMessage = "In/out argument \"" + file + "\" is of an invalid format.";
        //                    valid = false;
        //                    return;
        //                }
        //            }
        //            else if (data.Length == 2)
        //            {
        //                target = data[1];
        //            }

        //            inputs.Add(path);

        //            if (!options.SingleFileOutputMode)
        //            {
        //                outputs.Add(target);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        valid = false;
        //    }
        //}

        private static void ValidatePath(ref bool valid, ref string errorMessage, string path)
        {
            try
            {
                Path.GetFullPath(path);

                if (!File.Exists(path))
                {
                    errorMessage = "File \"" + path + "\" does not exist.";
                    valid = false;
                }
            }
            catch (ArgumentException)
            {
                errorMessage = "The path \"" + path + "\" is invalid.";
                valid = false;
            }
            catch (NotSupportedException)
            {
                errorMessage = "The path \"" + path + "\" is of an unsupported format.";
                valid = false;
            }
            catch (PathTooLongException)
            {
                errorMessage = "The path \"" + path + "\" is longer than the system-defined maximum length.";
                valid = false;
            }
        }

        static bool Compile(string filename, out string output)
        {
            output = null;
            string sourceFile = File.ReadAllText(filename);

            try
            {
                string compiled = JoopCompiler.Compile(sourceFile);
                output = compiled;
            }
            catch (JoopCompilerException e)
            {
                string nearToken;
                int lineNumber;
                int colNumber;
                JoopCompiler.GetNearTokenFromException(sourceFile, e, out nearToken, out lineNumber, out colNumber);

                LogError(e.Message, new ErrorPosition()
                {
                    Filename = filename,
                    LineNumber = lineNumber,
                    ColumnNumber = colNumber,
                    Length = nearToken.Length
                });
                return false;
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                return false;
            }

            return true;
        }

        static void LogError(string errorText, ErrorPosition position = default(ErrorPosition))
        {
            string prefix = "";

            if (!position.Equals(default(ErrorPosition)))
            {
                if (MSBuildErrors)
                {
                    prefix = position.Filename + "(" +
                        position.LineNumber + "," +
                        position.ColumnNumber + (position.Length > 0 ? "," + position.LineNumber + "," + (position.ColumnNumber + position.Length) : "") +
                        ") : error CERROR : ";
                }
                else
                {
                    prefix = position.Filename + " (Line " +
                        position.LineNumber + ", Column " +
                        position.ColumnNumber +
                        "): ";
                }
            }

            if (string.IsNullOrEmpty(prefix) && MSBuildErrors)
            {
                prefix = "JoopCompiler : error CERROR : ";
            }

            Console.WriteLine(prefix + errorText);
            
        }

        struct ErrorPosition
        {
            public string Filename
            {
                get;
                set;
            }

            public int LineNumber
            {
                get;
                set;
            }

            public int ColumnNumber
            {
                get;
                set;
            }

            public int Length
            {
                get;
                set;
            }
        }
    }
}
