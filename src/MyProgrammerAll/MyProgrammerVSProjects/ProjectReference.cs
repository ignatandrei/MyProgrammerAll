using MyProgrammerBase;

namespace MyProgrammerVSProjects
{
    [StructGenerators.GenerateToString()]
    public partial class ProjectReference : BaseUseApp
    {
        public string Source { get; set; }
        public string[] ProjectReferences { get; set; }

        public NuGetReference[] PackageReferences { get; set; }
    }
}
