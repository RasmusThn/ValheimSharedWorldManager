using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimSharedWorldManager.Models
{
    public sealed class BackupMetadata
    {
        public string World { get; set; } = "";
        public DateTime CreatedUtc { get; set; }

        // before_hosting, after_hosting, before_restore osv.
        public string Type { get; set; } = "";

        public string Machine { get; set; } = "";
        public string User { get; set; } = "";
    }
}
