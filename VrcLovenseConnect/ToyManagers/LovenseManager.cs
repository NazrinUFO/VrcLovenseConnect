using static LovenseConnect.LovenseConnectApi;

namespace VrcLovenseConnect.ToyManagers
{
    internal class LovenseManager : IToyManager
    {
        private bool disposedValue;
        readonly string address;
        List<LovenseToy> toys;

        public IEnumerable<string> ToyNames => toys.Select(toy => toy.Name);

        public bool IsToyFound => toys.Any();

        internal LovenseManager(string address)
        {
            this.address = address;
            toys = new List<LovenseToy>();
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
            // Finds connected toys.
            toys = await GetToys(address) ?? new List<LovenseToy>();
        }

        public async Task Vibrate(string toyName, float haptics)
        {
            var toy = toys.FirstOrDefault(t => t.Name == toyName);

            // Scales the received value to Lovense's Vibration scale (0-20).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 20.0f);

            // Vibrates the toy with the set intensity.
            if (toy != null && !toy.VibrateUnsupported)
            {
                try
                {
                    await VibrateToy(address, toy.Id ?? string.Empty, intensity, true);
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
            var toy = toys.FirstOrDefault(t => t.Name == toyName);

            // Scales the received value to Lovense's Rotation scale (0-20).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 20.0f);

            // Vibrates the toy with the set intensity.
            if (toy != null && !toy.RotateUnsupported)
            {
                try
                {
                    await RotateToy(address, toy?.Id ?? string.Empty, intensity, true);
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
            var toy = toys.FirstOrDefault(t => t.Name == toyName);

            // Scales the received value to Lovense's AutoAir scale (0-3).
            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
            int intensity = (int)Math.Ceiling(haptics * 3.0f);

            // Vibrates the toy with the set intensity.
            if (toy != null && !toy.LinearUnsupported)
            {
                try
                {
                    await PumpToy(address, toy?.Id ?? string.Empty, intensity, true);
                }
                catch
                {
                    toy.LinearUnsupported = true;
                }
            }
        }
    }
}
