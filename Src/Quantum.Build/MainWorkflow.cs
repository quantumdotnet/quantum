namespace Quantum.Build
{
    using System;
    using System.Linq;
    using Rosalia.Core.Api;
    using Rosalia.Core.Tasks.Futures;
    using Rosalia.Core.Tasks.Results;
    using Rosalia.FileSystem;
    using Rosalia.TaskLib.Git.Tasks;
    using Rosalia.TaskLib.MsBuild;
    using Rosalia.TaskLib.NuGet.Tasks;
    using Rosalia.TaskLib.Standard.Tasks;

    public class MainWorkflow : Workflow
    {
        public string Configuration { get; set; }

        protected override void RegisterTasks()
        {
            /* 
             * Init solution
             */
            var buildData = Task(
                "Init",
                context =>
                {
                    IDirectory src = context.WorkDirectory;
                    while (src != null && src.Name != "Src")
                    {
                        src = src.Parent;
                    }

                    if (src == null)
                    {
                        throw new Exception("Could not find Src directory");    
                    }

                    IDirectory artifacts = src.Parent/"Artifacts";
                    artifacts.EnsureExists();

                    // remove obsolete artifacts
                    artifacts
                        .Files
                        .IncludeByExtension("nuspec", "nupkg")
                        .DeleteAll();

                    return new
                    {
                        Configuration = Configuration ?? "Debug",
                        Src = src,
                        Artifacts = artifacts,
                        IsMono = context.Environment.IsMono,
                        WorkDirectory = context.WorkDirectory
                    }.AsTaskResult();
                });

            var solutionVersionTask = Task(
                "gitVersion",
                new GetVersionTask().TransformWith(gitVersion => new
                {
                    SolutionVersion = gitVersion.Tag.Replace("v", string.Empty) + ".0." + gitVersion.CommitsCount,
                    NuGetVersion = gitVersion.Tag.Replace("v", string.Empty) + ".0." + gitVersion.CommitsCount + "-alpha"
                }));

            ITaskFuture<Nothing> buildBuilderTask = Task(
                "BuildBuilderFactoryProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src/"Quantum.BuilderFactory"/"Quantum.BuilderFactory.fsproj",
                    Switches =
                    {
                        MsBuildSwitch.Configuration(data.Configuration),
                        MsBuildSwitch.VerbosityMinimal()
                    }
                }.AsTask(),
                
                DependsOn(solutionVersionTask));

            ITaskFuture<Nothing> buildBasisTask = Task(
                "BuildSqlBasisProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src/"Quantum.QueryBuilder.MsSql"/"Quantum.QueryBuilder.MsSql.csproj",
                    Switches =
                    {
                        MsBuildSwitch.Configuration(data.Configuration),
                        MsBuildSwitch.VerbosityMinimal()
                    }
                }.AsTask());

            var runMetaCodegen = Task(
                "RunBuilderCodeGen",
                from data in buildData
                select new ExecTask
                {
                    ToolPath = data.Src/"Quantum.BuilderFactory"/"bin"/data.Configuration/"Quantum.BuilderFactory.exe",
                    Arguments = string.Format(
                        "{0} {1} {2}",
                        data.Src/"Quantum.QueryBuilder.MsSql"/"bin"/data.Configuration/"Quantum.QueryBuilder.MsSql.dll",
                        data.Src/"Quantum.QueryBuilder.MsSql"/"map.txt",
                        data.Src/"testOut.cs"),
                    WorkDirectory = data.Src/"Quantum.BuilderFactory"/"bin"/data.Configuration
                }.AsTask(),
                
                DependsOn(buildBuilderTask),
                DependsOn(buildBasisTask));

            ITaskFuture<Nothing> buildGeneratedCodeProject = Task(
                "BuildSqlGeneratedProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src/"Quantum.QueryBuilder.MsSql.Generated"/"Quantum.QueryBuilder.MsSql.Generated.csproj",
                    Switches =
                    {
                        MsBuildSwitch.Configuration(data.Configuration),
                        MsBuildSwitch.VerbosityMinimal()
                    }
                }.AsTask(),
                
                DependsOn(runMetaCodegen));

            ITaskFuture<Nothing> ilMergeTask = Task(
                "ILMerge",
                from data in buildData
                select new ExecTask
                {
                    ToolPath = (data.Src/"packages").AsDirectory().Directories.Last(d => d.Name.StartsWith("ILRepack"))/"tools"/"ILRepack.exe",
                    Arguments = string.Format(
                        "/out:{0} {1}", 
                        data.Artifacts/"Quantum.dll",
                        string.Join(" ", 
                            (data.Src/"Quantum.QueryBuilder.MsSql.Generated"/"bin"/data.Configuration)
                                .AsDirectory()
                                .Files
                                .Include(f => f.Extension.Is("dll"))
                                .Select(f => f.AbsolutePath)
                            )
                        )
                }.AsTask(),
                
                DependsOn(buildGeneratedCodeProject));

            /* ======================================================================================== */

            var nuspecQuantum = Task(
                "nuspecRosaliaExe",

                from data in buildData
                from version in solutionVersionTask
                select new GenerateNuGetSpecTask(data.Artifacts/"Quantum.nuspec")
                    .Id("Quantum")
                    .Version(version.NuGetVersion)
                    .Authors("Eugene Guryanov")
                    .Description("Quantum is a SQL builder library for C#")
                    .WithFile(data.Artifacts/"Quantum.dll", "lib")
                    .AsTask(),

                DependsOn(ilMergeTask));


            var nuspecQuantumCodegen = Task(
                "nuspecQuantumCodegen",

                from data in buildData
                from version in solutionVersionTask
                select new GenerateNuGetSpecTask(data.Artifacts/"Quantum.CodeGen.nuspec")
                    .Id("Quantum.CodeGen")
                    .Version(version.NuGetVersion)
                    .Authors("Eugene Guryanov, ordis")
                    .Description("Model code generator for Quantum.NET")
                    .WithFile(data.Src/"Quantum.Generator"/"bin"/data.Configuration/"Quantum.Generator.exe", "tools")
                    .WithFile(data.Src/"Quantum.Build"/"Assets"/"init.ps1", "tools")
                    .AsTask(),

                DependsOn(nuspecQuantum));

            /* ======================================================================================== */

            var generateNuGetPackages = Task(
                "Generate NuGet packages",
                from data in buildData
                select ForEach(data.Artifacts.Files.IncludeByExtension(".nuspec")).Do(
                    file => new GeneratePackageTask(file)
                    {
                        ToolPath = data.Src/".nuget"/"NuGet.exe"
                    },
                    file => "pack " + file.Name),

                DependsOn(nuspecQuantum),
                DependsOn(nuspecQuantumCodegen));

            /* ======================================================================================== */

            Task(
                "pushNuGetPackages",
                from data in buildData
                select
                    ForEach(data.Artifacts.Files.IncludeByExtension(".nupkg")).Do(
                        task: file => new PushPackageTask(file)
                        {
                            ToolPath = data.Src/".nuget"/"NuGet.exe"
                        },
                        name: file => "push" + file.NameWithoutExtension),

                DependsOn(generateNuGetPackages));
        }
    }
}
