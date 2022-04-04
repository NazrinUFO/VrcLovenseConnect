using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect.Helpers
{
    internal class ToyConfig
    {
        /// <summary>
        /// The toy's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The protocol to use for toy controls.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for vibration commands.
        /// </summary>
        public string VibrateParameter { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for vibration commands.
        /// </summary>
        [JsonIgnore]
        public string VibrateAddress => $"/avatar/parameters/{VibrateParameter}";

        /// <summary>
        /// The intensity for vibrations (0.0 to 1.0, boolean only).
        /// </summary>
        public float VibrateIntensity { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for pumping/linear commands.
        /// </summary>
        public string PumpParameter { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for pumping/linear commands.
        /// </summary>
        [JsonIgnore]
        public string PumpAddress => $"/avatar/parameters/{PumpParameter}";

        /// <summary>
        /// The intensity for pumping (0.0 to 1.0, boolean Contacts only).
        /// </summary>
        public float PumpIntensity { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for rotation commands.
        /// </summary>
        public string RotateParameter { get; set; }

        /// <summary>
        /// The Avatar Parameter to synchronize with for rotation commands.
        /// </summary>
        [JsonIgnore]
        public string RotateAddress => $"/avatar/parameters/{RotateParameter}";

        /// <summary>
        /// The intensity for rotations (0.0 to 1.0, boolean Contacts only).
        /// </summary>
        public float RotateIntensity { get; set; }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ToyConfig()
        {
            Name = string.Empty;
            Protocol = string.Empty;
            VibrateParameter = string.Empty;
            VibrateIntensity = 0;
            PumpParameter = string.Empty;
            PumpIntensity = 0;
            RotateParameter = string.Empty;
            RotateIntensity = 0;
        }

        public bool ControlParameters()
        {
            if (string.IsNullOrWhiteSpace(VibrateParameter))
            {
                ConsoleHelper.PrintError("Vibration Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (VibrateIntensity <= 0)
            {
                ConsoleHelper.PrintError("Vibration intensity error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PumpParameter))
            {
                ConsoleHelper.PrintError("Pumping Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (PumpIntensity <= 0)
            {
                ConsoleHelper.PrintError("Pumping intensity error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (string.IsNullOrWhiteSpace(RotateParameter))
            {
                ConsoleHelper.PrintError("Rotation Avatar Parameter error in configuration file. Please enter a valid name.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            if (RotateIntensity <= 0)
            {
                ConsoleHelper.PrintError("Rotation intensity error in configuration file. Pleaser enter a non-zero, positive value.");
                ConsoleHelper.AwaitUserKeyPress();
                return false;
            }

            return true;
        }
    }
}
