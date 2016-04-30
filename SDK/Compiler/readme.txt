Note: You can also see this document in the command line through joopc --help, joopc -?, or just joopc. 
---------- 
joopc (Joop Compiler)
Compiles *.joop files into *.js files.

Usage:
    joopc [-m] [mode] [options]
    joopc [-m] -d [-r] -i (Directory) [-s (Output file) | -a]
    joopc [-m] -f [-a] [-i (Input File) -o (Output File) [-i  [-o] ..] ..]
    
    Modes:
        -d | --dirmode         Search a directory for *.joop files.
        -f | --filemode        Specify specific files for execution.
    
    Options:
        -m | --msbuild         Output errors in MSBuild-readable format.
        -a | --autoname        Output one file per *.joop file. The files are
                               automatically named based on the original file.
    
        Directory Mode
            -r | --rootonly    Only search the root directory. By default, the 
                               compiler searches the root directory and all sub
                               directories.
            -i | --indir       Required. The input directory.
            -s | --singlefile  Output one large *.js file.
            
        File Mode
            -i | --input       A file containing JOOP code as input.
            -o | --output      A filename to output the previously specified
                               input. Note: if the --autoname option is not
                               specified, this option is required.
    
Examples:
    joopc -f --input Test.joop --output Test.js
    This will compile a file called "Test.joop" and output the result to a file
    called "Test.js".
    
    joopc -d --indir \ -a
    This will compile all the files in the current directory, and output them
    to files with automatic names.

    joopc -f --autoname --input TestFile.joop
    This will compile a file called "TestFile.joop" and output the result to a
    file called "TestFile.js". The output file name is automatically created.
