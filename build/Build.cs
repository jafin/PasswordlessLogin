using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    string ArtifactsDirectory => "./artifacts";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "netcoreapp3.1")] readonly GitVersion GitVersion;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var projects = new List<string>
            {
                "PasswordlessLogin", "PasswordlessLogin.API", "PasswordlessLogin.SqlServer", "PasswordlessLogin.MySql"
            };

            EnsureCleanDirectory(ArtifactsDirectory);

            foreach (var project in projects)
            {
                DotNetPack(s => s
                    .SetProject(Solution.GetProject(project))
                    .SetConfiguration(Configuration)
                    .SetVersion(GitVersion.NuGetVersionV2)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    //.SetDescription("Sample package produced by NUKE")
                    //.SetPackageTags("nuke demonstration c# library")
                    .SetNoDependencies(true)
                    .SetOutputDirectory(ArtifactsDirectory)
                );
            }
        });
}