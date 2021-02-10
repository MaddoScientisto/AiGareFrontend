using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiGareFrontend
{
    public class Settings
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string CommandPath { get; set; }

        public string ExecuteFrom { get; set; }
        public bool? UseShellExecute { get; set; }
        public string Arguments { get; set; }
        public bool? Recursive { get; set; }
    }
}
