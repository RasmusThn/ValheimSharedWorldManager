using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimSharedWorldManager.Models
{
    public sealed class HostSessionInfo
    {
        public string World { get; set; } = "";

        public string JoinCode { get; set; } = "";

        public string Password { get; set; } = "";

        public DateTime UpdatedUtc { get; set; }

        public string HostMachine { get; set; } = "";

        public string HostUser { get; set; } = "";
    }
}
