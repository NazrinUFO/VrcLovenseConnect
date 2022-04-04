using Buttplug;

namespace VrcLovenseConnect.ToyManagers
{
    internal class ButtplugToy
    {
        public ButtplugClientDevice Toy { get; set; }
        public bool VibrateUnsupported { get; set; }
        public bool LinearUnsupported { get; set; }
        public bool RotateUnsupported { get; set; }

        public ButtplugToy(ButtplugClientDevice toy)
        {
            Toy = toy;
        }
    }
}
