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

        internal float? Haptics { get; set; }

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
                    OscPacket packet = new OscMessage(config.VibrateParameter, 0.1f);
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
                            retries = 0;

                            // Looks into the message every N-calls
                            if (nbrMessages == 0)
                            {
#if DEBUG
                                Console.WriteLine(message.ToString());
#endif
                                Haptics = message.FirstOrDefault() as float?;

                                if (Haptics.HasValue && Haptics.Value > 0)
                                {
                                    // Commands the toy.
                                    if (message.Address == config.VibrateParameter)
                                    {
                                        if (config.CommandAll)
                                        {
                                            await toyManager.Vibrate(Haptics.Value);
                                            await toyManager.Pump(Haptics.Value);
                                            await toyManager.Rotate(Haptics.Value);
                                        }
                                        else
                                        {
                                            await toyManager.Vibrate(Haptics.Value);
                                        }
                                    }
                                    else if (!config.CommandAll && message.Address == config.PumpParameter)
                                    {
                                        await toyManager.Pump(Haptics.Value);
                                    }
                                    else if (!config.CommandAll && message.Address == config.RotateParameter)
                                    {
                                        await toyManager.Rotate(Haptics.Value);
                                    }
                                }

                                // Next call.
                                nbrMessages++;
                            }
                            else
                            {
                                // Next call.
                                nbrMessages++;

                                // Resets the number of calls when limit is reached.
                                if (nbrMessages > config.Limit)
                                    nbrMessages = 0;
                            }
                        }
                        else
                        {
                            retries++;

                            // No message received for a moment, pauses vibration if started.
                            if (retries > config.Limit && Haptics.HasValue && Haptics.Value > 0)
                            {
                                Haptics = 0;

                                await toyManager.Vibrate(0);
                                await toyManager.Pump(0);
                                await toyManager.Rotate(0);
#if DEBUG
                                Console.WriteLine("Vibration stopped.");
#endif
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
    }
}
