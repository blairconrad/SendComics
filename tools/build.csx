#r "../tools/packages/Bullseye.1.0.0-rc.4/lib/netstandard2.0/Bullseye.dll"

using System.Runtime.CompilerServices;
using Bullseye;
using static Bullseye.Targets;

var solution = "./SendComics.sln";

// tool locations
static var toolsPackagesDirectory = Path.Combine(GetCurrentScriptDirectory(), "packages");
var vswhere = $"{toolsPackagesDirectory}/vswhere.2.4.1/tools/vswhere.exe";
var msBuild = $"{GetVSLocation()}/MSBuild/15.0/Bin/MSBuild.exe";
var xunit = $"{toolsPackagesDirectory}/xunit.runner.console.2.0.0/tools/xunit.console.exe";

// artifact locations
var logsDirectory = "./artifacts/logs";
static var testsDirectory = "./artifacts/tests";

// targets
Targets.Add("default", DependsOn("test"));

Targets.Add("logsDirectory", () => Directory.CreateDirectory(logsDirectory));

Targets.Add("testsDirectory", () => Directory.CreateDirectory(testsDirectory));

Targets.Add("build", DependsOn("clean", "restore"), () => RunMsBuild("Build"));

Targets.Add("clean", DependsOn("logsDirectory"), () => RunMsBuild("Clean"));

Targets.Add("restore", () => Cmd("dotnet", "restore"));

Targets.Add(
    "test",
    DependsOn("build", "testsDirectory"),
    () => RunTests("tests/SendComics.IntegrationTests/bin/Release/SendComics.UnitTests.dll"));

Targets.Run(Args);

// helpers
public static void Cmd(string fileName, string args)
{
    Cmd(".", fileName, args);
}

public static void Cmd(string workingDirectory, string fileName, string args)
{
    using (var process = new Process())
    {
        process.StartInfo = new ProcessStartInfo
        {
            FileName = $"\"{fileName}\"",
            Arguments = args,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };

        var workingDirectoryMessage = workingDirectory == "." ? "" : $" in '{process.StartInfo.WorkingDirectory}'";
        Console.WriteLine($"Running '{process.StartInfo.FileName} {process.StartInfo.Arguments}'{workingDirectoryMessage}...");
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The {fileName} command exited with code {process.ExitCode}.");
        }
    }
}

public string ReadCmdOutput(string workingDirectory, string fileName, string args)
{
    using (var process = new Process())
    {
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        var workingDirectoryMessage = workingDirectory == "." ? "" : $" in '{process.StartInfo.WorkingDirectory}'";
        Console.WriteLine($"Running '{process.StartInfo.FileName} {process.StartInfo.Arguments}'{workingDirectoryMessage}...");
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The {fileName} command exited with code {process.ExitCode}.");
        }

        return process.StandardOutput.ReadToEnd().Trim();
    }
}

public void RunMsBuild(string target)
{
    Cmd(
        msBuild,
        $"{solution} /target:{target} /p:configuration=Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:artifacts/logs/{target}.binlog");
}

public void RunTests(string testAssembly)
{
    var xml = Path.GetFullPath(Path.Combine(testsDirectory, Path.GetFileNameWithoutExtension(testAssembly) + ".TestResults.xml"));
    var html = Path.GetFullPath(Path.Combine(testsDirectory, Path.GetFileNameWithoutExtension(testAssembly) + ".TestResults.html"));
    Cmd(xunit, $"{testAssembly} -nologo -notrait \"explicit=yes\" -xml {xml} -html {html}");
}

public string GetVSLocation()
{
    var installationPath = ReadCmdOutput(".", $"\"{vswhere}\"", "-nologo -latest -property installationPath -requires Microsoft.Component.MSBuild -version [15,16)");
    if (string.IsNullOrEmpty(installationPath))
    {
        throw new InvalidOperationException("Visual Studio 2017 was not found");
    }

    return installationPath;
}

public static string GetCurrentScriptDirectory([CallerFilePath] string path = null) => Path.GetDirectoryName(path);
