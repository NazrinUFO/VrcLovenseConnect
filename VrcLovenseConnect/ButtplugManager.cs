using Buttplug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect
{
    internal class ButtplugManager : IToyManager
    {
        private bool disposedValue;
        readonly ButtplugClient client;
        ButtplugClientDevice? toy;
        readonly int scanTime;

        public string ToyName => toy?.Name ?? string.Empty;

        public bool IsToyFound => toy != null;

        internal ButtplugManager(int scanTime)
        {
            // Converts to milliseconds.
            this.scanTime = scanTime * 1000;

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
            if (toy != null)
                await toy.SendVibrateCmd(haptics);
        }
    }
}
