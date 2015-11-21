namespace Quantum.Build
{
    using System;
    using System.Linq;
    using Rosalia.Core.Api;
    using Rosalia.Core.Tasks.Futures;
    using Rosalia.Core.Tasks.Results;
    using Rosalia.FileSystem;
    using Rosalia.TaskLib.MsBuild;
    using Rosalia.TaskLib.Standard.Tasks;

    public class MainWorkflow : Workflow
    {
        public string Configuration { get; set; }

        protected override void RegisterTasks()
        {
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

                    IDirectory artifacts = src.Parent["Artifacts"];
                    artifacts.EnsureExists();

                    return new
                    {
                        Configuration = Configuration ?? "Debug",
                        Src = src,
                        Artifacts = artifacts,
                        IsMono = context.Environment.IsMono,
                        WorkDirectory = context.WorkDirectory
                    }.AsTaskResult();
                });

            ITaskFuture<Nothing> buildBuilderTask = Task(
                "BuildBuilderFactoryProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src["Quantum.BuilderFactory"]["Quantum.BuilderFactory.fsproj"],
                    Switches =
                    {
                        MsBuildSwitch.Configuration(data.Configuration),
                        MsBuildSwitch.VerbosityMinimal()
                    }
                }.AsTask());

            ITaskFuture<Nothing> buildBasisTask = Task(
                "BuildSqlBasisProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src["Quantum.QueryBuilder.MsSql"]["Quantum.QueryBuilder.MsSql.csproj"],
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
                    ToolPath = data.Src["Quantum.BuilderFactory"]["bin"][data.Configuration]["Quantum.BuilderFactory.exe"].AsFile().AbsolutePath,
                    Arguments = string.Format(
                        "{0} {1} {2}",
                        data.Src["Quantum.QueryBuilder.MsSql"]["bin"][data.Configuration]["Quantum.QueryBuilder.MsSql.dll"].AsFile().AbsolutePath,
                        data.Src["Quantum.QueryBuilder.MsSql"]["map.txt"].AsFile().AbsolutePath,
                        data.Src["testOut.cs"].AsFile().AbsolutePath),
                    WorkDirectory = data.Src["Quantum.BuilderFactory"]["bin"][data.Configuration]
                }.AsTask(),
                
                DependsOn(buildBuilderTask),
                DependsOn(buildBasisTask));

            ITaskFuture<Nothing> buildGeneratedCodeProject = Task(
                "BuildSqlGeneratedProject",
                from data in buildData
                select new MsBuildTask
                {
                    ProjectFile = data.Src["Quantum.QueryBuilder.MsSql.Generated"]["Quantum.QueryBuilder.MsSql.Generated.csproj"],
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
                    ToolPath = (data.IsMono ? "mono " : "") + data.Src["packages"].AsDirectory().Directories.Last(d => d.Name.StartsWith("ILRepack")).GetDirectory("tools").GetFile("ILRepack.exe").GetRelativePath(data.WorkDirectory),
                    Arguments = string.Format(
                        "/out:{0} {1}", 
                        data.Artifacts["Quantum.dll"].AsFile().GetRelativePath(data.WorkDirectory),
                        string.Join(" ", 
                            data.Src["Quantum.QueryBuilder.MsSql.Generated"]["bin"][data.Configuration]
                                .AsDirectory()
                                .SearchFilesIn()
                                .Include(f => f.Extension.Is("dll"))
                                .Select(f => f.GetRelativePath(data.WorkDirectory))
                            ))
                }.AsTask(),
                
                DependsOn(buildGeneratedCodeProject));
        }
    }
}