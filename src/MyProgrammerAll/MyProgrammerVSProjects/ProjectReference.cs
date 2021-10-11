using MyProgrammerBase;

namespace MyProgrammerVSProjects
{
    [StructGenerators.GenerateToString()]
    public class ProjectReference : BaseUseApp
    {
        public string Source { get; set; }
        public string[] ProjectReferences { get; set; }

        public NuGetReference[] PackageReferences { get; set; }
    }
}
