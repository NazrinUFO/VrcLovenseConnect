using Rug.Osc;
using System.Net;
using VrcLovenseConnect.ToyManagers;

namespace VrcLovenseConnect.Helpers
{
    internal class OscModule
    {
        readonly Config config;
        readonly IToyManager toyManager;
        double retries;
        int nbrMessages;
        bool vibrateUnsupported;
        bool linearUnsupported;
        bool rotateUnsupported;

        internal bool Play { get; set; } = true;

        internal float Haptics { get; set; }

        internal bool IsBooleanContact { get; set; }

        internal OscModule(Config config, IToyManager toyManager)
        {
            this.config = config;
            this.toyManager = toyManager;
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
                    //OscPacket packet = new OscMessage(config.VibrateParameter, 0.1f);
                    OscPacket packet = new OscMessage(config.VibrateParameter, true);
#else
                    messageReceived = oscReceiver.TryReceive(out OscPacket packet);
#endif

                    // Message received, sends intensity to command the toy.
                    if (messageReceived)
                    {
                        message = packet as OscMessage;

                        // If a specified Avatar Parameter is received, fetches its value.
                        if (message != null && (message.Address == config.VibrateParameter
                            || (!config.CommandAll
                                && (message.Address == config.PumpParameter
                                || message.Address == config.RotateParameter))))
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

                                    retries = 0;

                                    // Controls the toy.
                                    if (message.Address == config.VibrateParameter)
                                    {
                                        if (config.CommandAll)
                                        {
                                            await toyManager.Vibrate(IsBooleanContact ? config.VibrateIntensity : Haptics);
                                            await toyManager.Pump(IsBooleanContact ? config.PumpIntensity : Haptics);
                                            await toyManager.Rotate(IsBooleanContact ? config.RotateIntensity : Haptics);
                                        }
                                        else
                                        {
                                            await toyManager.Vibrate(Haptics);
                                        }
                                    }
                                    else if (!config.CommandAll && message.Address == config.PumpParameter)
                                    {
                                        await toyManager.Pump(IsBooleanContact ? config.PumpIntensity : Haptics);
                                    }
                                    else if (!config.CommandAll && message.Address == config.RotateParameter)
                                    {
                                        await toyManager.Rotate(IsBooleanContact ? config.RotateIntensity : Haptics);
                                    }

                                    // Message processed, the next N messages will be skipped for performance.
                                    CountMessages();
                                }
                                else
                                {
                                    // Message has an invalid value, this counts as a no-concern.
                                    await CountRetries();
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
                            // The received message doesn't concern VRCLovenseConnect.
                            await CountRetries();
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

        internal async Task StopToy()
        {
            IsBooleanContact = false;
            Haptics = 0;

            await toyManager.Vibrate(0);
            await toyManager.Pump(0);
            await toyManager.Rotate(0);
#if DEBUG
            Console.WriteLine("Vibration stopped.");
#endif
        }

        private async Task CountRetries()
        {
            retries++;

            // No message received for a moment, pauses vibration if started.
            if (retries > config.Limit && (IsBooleanContact || Haptics > 0))
                await StopToy();
        }

        private void CountMessages()
        {
            nbrMessages++;

            // Resets the number of messages read when limit is reached.
            if (nbrMessages > config.Limit)
                nbrMessages = 0;
        }
    }
}