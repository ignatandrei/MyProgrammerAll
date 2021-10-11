using BoilerplateFree;
using System;

namespace MyProgrammerBase
{
    [AutoGenerateInterface]
    public partial class BaseUseApp: IBaseUseApp
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public string HomeURL { get; set; }

    }
}
