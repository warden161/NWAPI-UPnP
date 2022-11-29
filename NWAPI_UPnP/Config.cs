using System.Diagnostics;
using System.ComponentModel;

namespace NWAPI_UPnP
{
    public sealed class Config
    {
        public bool IsEnabled { get; set; } = true;
        [Description("All debug levels can be found at: https://github.com/warden161/NWAPI_UPnP/blob/master/README.md#Debug")]
        public SourceLevels DebugLevel { get; set; } = SourceLevels.Warning;
        public string MappingName { get; set; } = "scpsl:%port%";
        public int Timeout { get; set; } = 5000;
    }
}