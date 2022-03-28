using Rug.Osc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect
{
    internal class OscModule
    {
        readonly Config config;
        readonly IToyManager toyManager;
        double retries;
        int nbrMessages;
        
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
                // Listens for one tick. Non-blocking.
#if DEBUG
                messageReceived = true;
                OscPacket packet = new OscMessage($"/avatar/parameters/{config.Parameter}", 0.1f);
#else
                messageReceived = oscReceiver.TryReceive(out OscPacket packet);
#endif

                // Message received, sends intensity to the toy's vibration.
                if (messageReceived)
                {
                    message = packet as OscMessage;

                    // If the specified Avatar Parameter is received, fetches its value.
                    if (message?.Address == $"/avatar/parameters/{config.Parameter}")
                    {
                        retries = 0;

                        // Looks into the message every N-calls
                        if (nbrMessages == 0)
                        {
#if DEBUG
                            Console.WriteLine(message?.ToString());
#endif
                            Haptics = message?.FirstOrDefault() as float?;

                            if (Haptics.HasValue)
                            {
                                // Updates the toy to the vibration value.
                                await toyManager.Vibrate(Haptics.Value);
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
                            await toyManager.Vibrate(Haptics.Value);
#if DEBUG
                            Console.WriteLine("Vibration stopped.");
#endif
                        }
                    }
                }
            }
        }
    }
}
