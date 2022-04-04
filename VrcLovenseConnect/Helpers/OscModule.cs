using Rug.Osc;
using System.Net;
using VrcLovenseConnect.ToyManagers;

namespace VrcLovenseConnect.Helpers
{
    internal class OscModule
    {
        readonly Config config;
        readonly IToyManager lovenseManager;
        readonly IToyManager buttplugManager;
        IToyManager toyManager;
        readonly Dictionary<string, double> retries;
        int nbrMessages;

        internal bool Play { get; set; } = true;

        internal float Haptics { get; set; }

        internal bool IsBooleanContact { get; set; }

        internal OscModule(Config config, IToyManager lovenseManager, IToyManager buttplugManager)
        {
            this.config = config;
            this.lovenseManager = lovenseManager;
            this.buttplugManager = buttplugManager;
            toyManager = buttplugManager;
            retries = new Dictionary<string, double>();
        }

        /// <summary>
        /// Listens to OSC messages and updates toys.
        /// </summary>
        internal async Task Listen()
        {
            OscMessage? message;
            bool messageReceived;

            // Listens for OSC messages on localhost, port 9001.
            using var oscReceiver = new OscReceiver(IPAddress.Loopback, config.OscPort);
            oscReceiver.Connect();
            Console.WriteLine($"Connected and listening to {oscReceiver.LocalAddress}:{oscReceiver.RemoteEndPoint.Port}...");

            // Loops until the program is closed.
            while (Play)
            {
                try
                {
                    // Listens for one tick. Non-blocking.
#if DEBUG
                    messageReceived = true;
                    OscPacket packet = new OscMessage("/avatar/parameters/LovenseHaptics", 0.1f);
                    //OscPacket packet = new OscMessage("/avatar/parameters/LovenseHaptics", true);
#else
                    messageReceived = oscReceiver.TryReceive(out OscPacket packet);
#endif

                    // Message received, sends intensity to command the toy.
                    if (messageReceived)
                    {
                        message = packet as OscMessage;

                        // Browses all connected toys.
                        foreach (var toy in config.Toys)
                        {
                            // If an Avatar Parameter for controlling toys is received, fetches its value.
                            if (message != null && (message.Address == toy.VibrateAddress
                            || message.Address == toy.PumpAddress
                            || message.Address == toy.RotateAddress))
                            {
                                // Looks into every N message.
                                if (nbrMessages == 0)
                                {
                                    // Reads the message's value and determines its type.
                                    IsBooleanContact = message.FirstOrDefault() as bool? ?? false;
                                    Haptics = IsBooleanContact ? 0.0f : (message.FirstOrDefault() as float? ?? 0.0f);

                                    if (IsBooleanContact || Haptics > 0)
                                    {
#if DEBUG
                                        Console.WriteLine(message.ToString());
#endif
                                        // Resets retries for this toy.
                                        retries[toy.Name] = 0;

                                        // Controls the toy.
                                        await CommandToy(toy, message);

                                        // Message processed, the next N messages will be skipped for performance.
                                        CountMessages();
                                    }
                                    else
                                    {
                                        // Message has an invalid value, this counts as a no-concern.
                                        await CountRetries(toy);
                                    }
                                }
                                else
                                {
                                    // Skipping N messages for performance.
                                    CountMessages();
                                }
                            }
                            else
                            {
                                // The received message doesn't concern this toy.
                                await CountRetries(toy);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // No critical error that requires a full stop.
#if DEBUG
                    ConsoleHelper.PrintError(ex.Message);
#endif
                }
            }
        }

        internal async Task StopToy(ToyConfig toy)
        {
            toyManager = toy.Protocol == "Lovense" ? lovenseManager : buttplugManager;

            IsBooleanContact = false;
            Haptics = 0;

            await toyManager.Vibrate(toy.Name, 0);
            await toyManager.Pump(toy.Name, 0);
            await toyManager.Rotate(toy.Name, 0);
#if DEBUG
            Console.WriteLine("Vibration stopped.");
#endif
        }

        private async Task CountRetries(ToyConfig toy)
        {
            // Counts the number of retries for this toy.
            retries[toy.Name]++;

            // No message received for a moment, pauses the toy if started.
            if (retries[toy.Name] > config.Limit && !IsBooleanContact && Haptics > 0)
                await StopToy(toy);
        }

        private void CountMessages()
        {
            // Counts the number of OSC messages received since the last process.
            nbrMessages++;

            // Resets the number of messages read when limit is reached.
            if (nbrMessages > config.Limit)
                nbrMessages = 0;
        }

        private async Task CommandToy(ToyConfig toy, OscMessage message)
        {
            toyManager = toy.Protocol == "Lovense" ? lovenseManager : buttplugManager;

            if (message.Address == toy.VibrateAddress)
            {
                await toyManager.Vibrate(toy.Name, IsBooleanContact ? toy.VibrateIntensity : Haptics);
            }
            if (message.Address == toy.PumpAddress)
            {
                await toyManager.Pump(toy.Name, IsBooleanContact ? toy.PumpIntensity : Haptics);
            }
            if (message.Address == toy.RotateAddress)
            {
                await toyManager.Rotate(toy.Name, IsBooleanContact ? toy.RotateIntensity : Haptics);
            }
        }
    }
}