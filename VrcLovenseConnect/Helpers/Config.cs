using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect.Helpers
{
    internal class Config
    {
        /// <summary>
        /// The port to listen for OSC messages.
        /// </summary>
        public int OscPort { get; set; }

        /// <summary>
        /// The address provided by Lovense Connect.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The time to scan for a toy in seconds.
        /// </summary>
        public int ScanTime { get; set; }

        /// <summary>
        /// The time to scan for a toy in milliseconds.
        /// </summary>
        [JsonIgnore]
        public int RuntimeScanTime => ScanTime * 1000;

        /// <summary>
        /// The number of received messages to skip.
        /// </summary>
        public int Limit { get; set; }

        public List<ToyConfig> Toys { get; set; }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Config()
        {
            Address = string.Empty;
            Limit = 0;
            Toys = new List<ToyConfig>();
        }

        public bool ControlParameters()
        {
            if (OscPort <= 0)
            {
                ConsoleHelper.PrintError("Port error in configuration file. Pleaser enter a valid port number.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Limit <= 0)
            {
                ConsoleHelper.PrintError("Limit error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                ConsoleHelper.PrintError("Address error in configuration file. Please enter the address provided by the Lovense Connect app on your phone or enter any valid address if unused.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (ScanTime <= 0)
            {
                ConsoleHelper.PrintError("Scan time error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            foreach (var toy in Toys)
                if (!toy.ControlParameters())
                    return false;

            return true;
        }
    }
}
