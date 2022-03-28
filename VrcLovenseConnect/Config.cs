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

        public bool ControlParameters()
        {
            if (OscPort <= 0)
            {
                Console.WriteLine("Port error in configuration file. Pleaser enter a valid port number.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Protocol == "Lovense" && string.IsNullOrWhiteSpace(Address))
            {
                Console.WriteLine("Address error in configuration file. Please enter the address provided by the Lovense Connect app on your phone.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Protocol == "Buttplug" && ScanTime <= 0)
            {
                Console.WriteLine("Scan time error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Limit <= 0)
            {
                Console.WriteLine("Limit error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Parameter))
            {
                Console.WriteLine("Avatar Parameter error in configuration file. Please enter a valid parameter name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            return true;
        }
    }
}
