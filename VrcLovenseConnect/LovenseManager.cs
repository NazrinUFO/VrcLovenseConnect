using static LovenseConnectAPI.LovenseConnect;

namespace VrcLovenseConnect
{
    internal class LovenseManager : IToyManager
    {
        private bool disposedValue;
        readonly string address;
        LovenseToy? toy;

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
            await VibrateToy(address, toy?.Id ?? string.Empty, intensity);
        }
    }
}
