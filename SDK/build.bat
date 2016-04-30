@echo off
echo Copying the latest joopc from the Joop.Compiler\bin\Release\Single\ folder...
del "Compiler\joopc.exe"
copy "..\Joop.Compiler\bin\Release\Single\joopc.exe" "Compiler\joopc.exe" /Y

echo Building the readme...
echo Note: You can also see this document in the command line through joopc --help, joopc -?, or just joopc. > "Compiler\readme.txt"
echo ---------- >> "Compiler\readme.txt"
"Compiler\joopc.exe" --? >> "Compiler\readme.txt"

echo Building the user guide...
rem Delete any existing guide
del "JOOP, In and Out (User Guide).docx"

rem Build the guide
echo Building the guide...
"%AppData%\..\Local\Pandoc\Pandoc.exe" -o "JOOP, In and Out (User Guide).docx" "JOOP, In and Out (User Guide).txt"

echo Packaging...
rem Delete old package
del "SDK.zip"
zip -r "SDK.zip" "Compiler" "IDE" "TryJoop" "JOOP, In and Out (User Guide).docx" "JOOP, In and Out (User Guide).txt" "Read the user guide.txt"
pause