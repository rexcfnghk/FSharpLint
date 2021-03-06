#### Using the Console Application

The console application is a very simple wrapper around the lint tool, with a required argument of `-f`, `-sf`, or `-source`. `-f` lints against an entire project, `-sf` can be used to lint a single file, and `-source` lints against a given string.

If you get a `ParseException` reported when running the tool it's likely that your `FSharp.Core.dll` is in a non-standard directory - if this is not the case please report it as an issue. When it *is* in a non-standard directory you can use the `-core` argument to specify the directory where your `FSharp.Core.dll` is located.

##### Example

`C:\FSharpLint\bin` is the directory containing `fsharplint.exe`

`C:\FSharpLint.FunctionalTest.TestedProject\FSharpLint.FunctionalTest.TestedProject.fsproj` is the path of the project file of the project that we want to lint.

To run the tool we need to open the command line, `cd` into the directory containing `fsharplint.exe` and then run the tool using the `-f` argument to specify the project file:

    >cd C:\FSharpLint\bin
    >fsharplint -f "C:\FSharpLint.FunctionalTest.TestedProject\FSharpLint.FunctionalTest.TestedProject.fsproj"

###### Output

![Example run](http://i.imgur.com/0DZTdDR.png "Example run of the console application")

#### Application Arguments

| Arguments | Description | Example |
| --- | :------------- | --- |
| `-f` | Specifies the path to the project file of the project to be linted. **(Required if -sf or source not specified)** | `-f "C:\FSharpProjectDirectory\ProjectFile.fsproj"` |
| `-sf` | Specifies the path to a single source file to be linted. **(Required if -f or -source not specified)** | `-f "C:\FSharpProjectDirectory\Uploader.fsx"` |
| `-source` | Lints a given string **(Required if -f or -sf not specified)** | `-source "let x = fun _ -> ()"` |
| `-core` | Specifies the path to the directory containing `FSharp.Core.dll` to be used to parse the project, this directory must contain `FSharp.Core.sigdata` and `FSharp.Core.optdata` alongside `FSharp.Core.dll` **(Optional)** | `-core "C:\Program Files (x86)\Reference Assemblies\Microsoft\FSharp\3.0\Runtime\v4.0"` |