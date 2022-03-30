using Buttplug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect.ToyManagers
{
    internal class ButtplugManager : IToyManager
    {
        private bool disposedValue;
        readonly ButtplugClient client;
        ButtplugClientDevice? toy;
        readonly int scanTime;
        readonly uint moveSpeed;
        readonly Dictionary<string, float> currentHaptics = new Dictionary<string, float>();
        bool vibrateUnsupported, linearUnsupported, rotateUnsupported;

        public string ToyName => toy?.Name ?? string.Empty;

        public bool IsToyFound => toy != null;

        internal ButtplugManager(int scanTime, uint moveSpeed)
        {
            // Converts to milliseconds.
            this.scanTime = scanTime;
            this.moveSpeed = moveSpeed;

            client = new ButtplugClient("MainClient");

            client.DeviceAdded += async (obj, args) =>
            {
                // Stops scanning whenever a device is found.
                await client.StopScanningAsync();
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    client.Dispose();
                
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task FindToy()
        {
            await client.ConnectAsync(new ButtplugEmbeddedConnectorOptions());
            client.StartScanningAsync().Wait();
            Thread.Sleep(scanTime);
            toy = client.Devices.FirstOrDefault();
        }

        public async Task Vibrate(float haptics)
        {
            if (!vibrateUnsupported && !currentHaptics.ContainsKey("Vibrate") || currentHaptics["Vibrate"] != haptics)
            {
                currentHaptics["Vibrate"] = haptics;

                if (toy != null)
                {
                    try
                    {
                        await toy.SendVibrateCmd(haptics);
                    }
                    catch
                    {
                        // If any error happens, disables the feature for safety.
                        vibrateUnsupported = true;
                    }
                }
            }
        }

        public async Task Rotate(float haptics)
        {
            if (!rotateUnsupported && !currentHaptics.ContainsKey("Rotate") || currentHaptics["Rotate"] != haptics)
            {
                currentHaptics["Rotate"] = haptics;

                if (toy != null)
                {
                    try
                    {
                        await toy.SendRotateCmd(haptics, true);
                    }
                    catch
                    {
                        // If any error happens, disables the feature for safety.
                        rotateUnsupported = true;
                    }
                }
            }
        }

        public async Task Pump(float haptics)
        {
            if (!linearUnsupported && !currentHaptics.ContainsKey("Pump") || currentHaptics["Pump"] != haptics)
            {
                currentHaptics["Pump"] = haptics;

                if (toy != null)
                {
                    try
                    {
                        await toy.SendLinearCmd(moveSpeed, haptics);
                    }
                    catch
                    {
                        // If any error happens, disables the feature for safety.
                        linearUnsupported = true;
                    }
                }
            }
        }
    }
}
