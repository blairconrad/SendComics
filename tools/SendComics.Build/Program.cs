namespace FakeItEasy.Build
{
    using System.Collections.Generic;
    using System.IO;
    using static Bullseye.Targets;
    using static SimpleExec.Command;

    public class Program
    {
        private const string Solution = "SendComics.sln";

        private const string TestProject = "tests/SendComics.IntegrationTests";

        private const string LogsDirectory = "artifacts/logs";

        public static void Main(string[] args)
        {
            Target("default", DependsOn("test"));

            Target("logsDirectory", () => Directory.CreateDirectory(LogsDirectory));

            Target(
                "build",
                () => Run("dotnet", $"build {Solution} -c Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:{LogsDirectory}/build.binlog"));

            Target("test", DependsOn("build"), () => RunTests(TestProject));

            RunTargets(args);
        }

        private static void RunTests(string testDirectory) =>
            Run("dotnet", "test --configuration Release --no-build -- RunConfiguration.NoAutoReporters=true", testDirectory);
    }
}
