using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect
{
    internal class Config
    {
        /// <summary>
        /// The port to listen for OSC messages.
        /// </summary>
        public int OscPort { get; set; }

        /// <summary>
        /// The protocol to use for toy controls.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The address provided by Lovense Connect.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The time to scan for a toy in milliseconds.
        /// </summary>
        public int ScanTime { get; set; }

        /// <summary>
        /// The number of received messages to skip.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with.
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Config()
        {
            Protocol = string.Empty;
            Address = string.Empty;
            Limit = 0;
            Parameter = string.Empty;
        }
    }
}
