using Buildalyzer.Construction;
using MyProgrammerBase;

namespace MyProgrammerVSProjects
{
    [StructGenerators.GenerateToString()]
    public partial class NuGetReference : BaseUseApp
    {
        public static NuGetReference FromPackageReference(IPackageReference item)
        {
            var packRef = new NuGetReference();
            packRef.Type = "package";
            packRef.ID = item.Name;
            packRef.Version = item.Version;
            return packRef;
        }
    }
}
