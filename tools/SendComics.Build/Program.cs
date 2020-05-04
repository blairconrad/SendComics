namespace FakeItEasy.Build
{
    using System.IO;
    using System.IO.Compression;
    using static Bullseye.Targets;
    using static SimpleExec.Command;

    public static class Program
    {
        private const string Solution = "SendComics.sln";

        private const string ResourceGroup = "sendcomics";

        private const string FunctionAppName = "sendcomics";

        private static readonly string MainProject = Path.Combine("src", "SendComics");

        private static readonly string TestProject = Path.Combine("tests", "SendComics.IntegrationTests");

        private static readonly string LogsDirectory = Path.Combine("artifacts", "logs");

        private static readonly string OutputDirectory = Path.Combine("artifacts", "output");

        private static readonly string PublishDirectory = Path.Combine(MainProject, "bin", "Release", "net461", "publish");

        private static readonly string PublishZip = Path.Combine(OutputDirectory, "SendComics.zip");

        public static void Main(string[] args)
        {
            Target("default", DependsOn("test"));

            Target("logsDirectory", () => Directory.CreateDirectory(LogsDirectory));
            Target("outputDirectory", () => Directory.CreateDirectory(OutputDirectory));

            Target(
                "build",
                () => Run("dotnet", $"build {Solution} -c Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:{LogsDirectory}/build.binlog"));

            Target("test", DependsOn("build"), () => RunTests(TestProject));

            Target(
                "publish",
                DependsOn("test"),
                () => Run("dotnet", $"publish --no-build -c Release {MainProject}"));

            Target(
                "zip",
                DependsOn("outputDirectory", "publish"),
                () =>
                    {
                        File.Delete(PublishZip);
                        ZipFile.CreateFromDirectory(PublishDirectory, PublishZip);
                    });

            Target(
                "deploy",
                DependsOn("zip"),
                () => Run("cmd.exe", $"/c az.cmd functionapp deployment source config-zip --resource-group {ResourceGroup} --name {FunctionAppName} --src {PublishZip}"));

            RunTargetsAndExit(args);
        }

        private static void RunTests(string testDirectory) =>
            Run("dotnet", "test --configuration Release --no-build -- RunConfiguration.NoAutoReporters=true", testDirectory);
    }
}
