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
        /// Allows all commands to be activated just by the Avatar Parameter for vibration, or with separate parameters.
        /// </summary>
        public bool CommandAll { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for vibration commands.
        /// </summary>
        public string VibrateParameter { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for pumping/linear commands.
        /// </summary>
        public string PumpParameter { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for rotation commands.
        /// </summary>
        public string RotateParameter { get; set; }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Config()
        {
            Protocol = string.Empty;
            Address = string.Empty;
            Limit = 0;
            CommandAll = false;
            VibrateParameter = string.Empty;
            PumpParameter = string.Empty;
            RotateParameter = string.Empty;
        }

        public bool ControlParameters()
        {
            if (OscPort <= 0)
            {
                ConsoleHelper.PrintError("Port error in configuration file. Pleaser enter a valid port number.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Protocol == "Lovense" && string.IsNullOrWhiteSpace(Address))
            {
                ConsoleHelper.PrintError("Address error in configuration file. Please enter the address provided by the Lovense Connect app on your phone.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Protocol == "Buttplug" && ScanTime <= 0)
            {
                ConsoleHelper.PrintError("Scan time error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (Limit <= 0)
            {
                ConsoleHelper.PrintError("Limit error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (string.IsNullOrWhiteSpace(VibrateParameter))
            {
                ConsoleHelper.PrintError("Vibration Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (!CommandAll && string.IsNullOrWhiteSpace(PumpParameter))
            {
                ConsoleHelper.PrintError("Pumping Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (!CommandAll && string.IsNullOrWhiteSpace(RotateParameter))
            {
                ConsoleHelper.PrintError("Rotation Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            // Converts scan time to milliseconds.
            ScanTime *= 1000;

            // Adds prefix to parameters.
            VibrateParameter = $"/avatar/parameters/{VibrateParameter}";
            PumpParameter = $"/avatar/parameters/{PumpParameter}";
            RotateParameter = $"/avatar/parameters/{RotateParameter}";

            return true;
        }
    }
}
