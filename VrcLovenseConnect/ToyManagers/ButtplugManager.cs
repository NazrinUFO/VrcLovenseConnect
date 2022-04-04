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
        List<ButtplugToy> toys;
        readonly int scanTime;
        readonly uint moveSpeed;
        readonly Dictionary<(string, string), float> currentHaptics = new Dictionary<(string, string), float>();

        public IEnumerable<string> ToyNames => toys.Select(toy => toy.Toy.Name);

        public bool IsToyFound => toys.Any();

        internal ButtplugManager(int scanTime, uint moveSpeed)
        {
            // Converts to milliseconds.
            this.scanTime = scanTime;
            this.moveSpeed = moveSpeed;

            client = new ButtplugClient("MainClient");

            toys = new List<ButtplugToy>();
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
            client.StopScanningAsync().Wait();

            toys = client.Devices.Select(toy => new ButtplugToy(toy)).ToList();
        }

        public async Task Vibrate(string toyName, float haptics)
        {
            var toy = toys.FirstOrDefault(t => t.Toy.Name == toyName);

            if (toy != null && !toy.VibrateUnsupported
                && (!currentHaptics.ContainsKey((toyName, "Vibrate")) || currentHaptics[(toyName, "Vibrate")] != haptics))
            {
                currentHaptics[(toyName, "Vibrate")] = haptics;

                try
                {
                    await toy.Toy.SendVibrateCmd(haptics);
                }
                catch
                {
                    // If any error happens, disables the feature for safety.
                    toy.VibrateUnsupported = true;
                }
            }
        }

        public async Task Rotate(string toyName, float haptics)
        {
            var toy = toys.FirstOrDefault(t => t.Toy.Name == toyName);

            if (toy != null && !toy.RotateUnsupported
                && (!currentHaptics.ContainsKey((toyName, "Rotate")) || currentHaptics[(toyName, "Rotate")] != haptics))
            {
                currentHaptics[(toyName, "Rotate")] = haptics;

                try
                {
                    await toy.Toy.SendRotateCmd(haptics, true);
                }
                catch
                {
                    // If any error happens, disables the feature for safety.
                    toy.RotateUnsupported = true;
                }
            }
        }

        public async Task Pump(string toyName, float haptics)
        {
            var toy = toys.FirstOrDefault(t => t.Toy.Name == toyName);

            if (toy != null && !toy.LinearUnsupported
                && (!currentHaptics.ContainsKey((toyName, "Pump")) || currentHaptics[(toyName, "Pump")] != haptics))
            {
                currentHaptics[(toyName, "Pump")] = haptics;

                try
                {
                    await toy.Toy.SendLinearCmd(moveSpeed, haptics);
                }
                catch
                {
                    // If any error happens, disables the feature for safety.
                    toy.LinearUnsupported = true;
                }
            }
        }
    }
}
