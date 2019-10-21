using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class RunnerTriggeredEventArgs : EventArgs
    {
        public int NextPlayer { get; set; }
    }
}
