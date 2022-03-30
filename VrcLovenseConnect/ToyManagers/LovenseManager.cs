using static LovenseConnect.LovenseConnectApi;

namespace VrcLovenseConnect.ToyManagers
{
    internal class LovenseManager : IToyManager
    {
        private bool disposedValue;
        readonly string address;
        LovenseToy? toy;
        bool vibrateUnsupported, linearUnsupported, rotateUnsupported;

        public string ToyName => toy?.Name ?? string.Empty;

        public bool IsToyFound => toy != null;

        internal LovenseManager(string address)
        {
            this.address = address;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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
            // Finds the first toy connected.
            var toys = await GetToys(address);
            toy = toys?.FirstOrDefault();
        }

        public async Task Vibrate(float haptics)
        {
            // Scales the received value to Lovense's Vibration scale (0-20).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 20.0f);

            // Vibrates the toy with the set intensity.
            if (!vibrateUnsupported)
            {
                try
                {
                    await VibrateToy(address, toy?.Id ?? string.Empty, intensity, true);
                }
                catch
                {
                    // If any error happens, disables the feature for safety.
                    vibrateUnsupported = true;
                }
            }
        }

        public async Task Rotate(float haptics)
        {
            // Scales the received value to Lovense's Rotation scale (0-20).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 20.0f);

            // Vibrates the toy with the set intensity.
            if (!rotateUnsupported)
            {
                try
                {
                    await RotateToy(address, toy?.Id ?? string.Empty, intensity, true);
                }
                catch
                {
                    // If any error happens, disables the feature for safety.
                    rotateUnsupported = true;
                }
            }
        }

        public async Task Pump(float haptics)
        {
            // Scales the received value to Lovense's AutoAir scale (0-3).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 3.0f);

            // Vibrates the toy with the set intensity.
            if (!linearUnsupported)
            {
                try
                {
                    await PumpToy(address, toy?.Id ?? string.Empty, intensity, true);
                }
                catch
                {
                    linearUnsupported = true;
                }
            }
        }
    }
}
