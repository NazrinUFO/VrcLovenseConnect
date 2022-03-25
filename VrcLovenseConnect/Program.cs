using LovenseConnectAPI;
using Newtonsoft.Json;
using Rug.Osc;
using System.Net;
using VrcLovenseConnect;
using static LovenseConnectAPI.LovenseConnect;

/// <summary>
/// The default text color in console.
/// </summary>
var defaultColor = Console.ForegroundColor;

/// <summary>
/// The vibration intensity.
/// </summary>
int intensity = 0;

/// <summary>
/// The connected toy.
/// </summary>
LovenseToy? toy = null;

/// <summary>
/// Indicates whether the program keeps running.
/// </summary>
bool play = true;

/// <summary>
/// Number of messages read.
/// </summary>
int nbrMessages = 0;

int retries = 0;

var configFile = File.ReadAllText("config.json");
var config = JsonConvert.DeserializeObject<Config>(configFile);

// Checks the config's parameters
if (config == null)
{
    Console.WriteLine("Error in configuration file. Please check the format.");
    Console.ReadKey(true);
    return;
}

if (config.OscPort <= 0)
{
    Console.WriteLine("Port error in configuration file. Pleaser enter a valid port number.");
    Console.ReadKey(true);
    return;
}

if (string.IsNullOrWhiteSpace(config.Address))
{
    Console.WriteLine("Address error in configuration file. Please enter the address provided by the Lovense Connect app on your phone.");
    Console.ReadKey(true);
    return;
}

if (config.Limit <= 0)
{
    Console.WriteLine("Limit error in configuration file. Pleaser enter a non-zero, positive value.");
    Console.ReadKey(true);
    return;
}

if (string.IsNullOrWhiteSpace(config.Parameter))
{
    Console.WriteLine("Avatar Parameter error in configuration file. Please enter a valid parameter name.");
    Console.ReadKey(true);
    return;
}

// Initiates the OSC receiver on localhost:9001.
using var receiver = new OscReceiver(IPAddress.Loopback, config.OscPort);

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("VRCLovenseConnect Alpha");
Console.ForegroundColor = defaultColor;

// Listens for OSC messages on localhost, port 9001.
receiver.Connect();
Console.WriteLine($"Connected and listening to {receiver.LocalAddress}:{receiver.RemoteEndPoint.Port}...");

// Once connected, starts a seperate thread to leave the console available for inputs.
var task = Task.Run(async () =>
{
    OscMessage? message;
    float? haptics;
    bool messageReceived;

    // Loops until the program is closed.
    while (play)
    {
        try
        {
            // Listens for one tick. Non-blocking.
            //messageReceived = receiver.TryReceive(out OscPacket packet);
            messageReceived = true;
            OscPacket packet = new OscMessage("/avatar/parameters/LovenseHaptics", 0.1f);

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
                        haptics = message?.FirstOrDefault() as float?;

                        if (haptics.HasValue)
                        {
                            // Scales the received value to Lovense's Vibration scale (0-20).
                            // Source: https://fr.lovense.com/sextoys/developer/doc#solution-3-cam-kit-step3
                            intensity = (int)Math.Ceiling(haptics.Value * 20.0f);

                            // Updates the toy to the vibration value.
                            await VibrateToy(intensity);
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
                    if (retries > config.Limit && intensity != 0 && toy != null)
                    {
                        intensity = 0;
                        await VibrateToy(intensity);
#if DEBUG
                        Console.WriteLine("Vibration stopped.");
#endif
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
});

// Any key press will close the program.
Console.ReadKey(true);

// Stops the program and waits for the loop to complete.
play = false;
task.Wait();

// Stops vibration if started.
if (intensity != 0 && toy != null)
    VibrateToy(0).Wait();

async Task VibrateToy(int intensity)
{
    try
    {
        // Finds the first toy connected.
        if (toy == null)
        {
            var toys = await LovenseConnect.GetToys(config.Address);
            toy = toys?.FirstOrDefault();

            if (toy == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Cannot find a toy! Please check that the address in the config.json file is the one provided by the Lovense Connect app on your phone.");
                Console.ForegroundColor = defaultColor;
                return;
            }
        }

        // Vibrates the toy with the set intensity.
        if (!await LovenseConnect.VibrateToy(config.Address, toy?.Id ?? string.Empty, intensity))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: Lovense update failed! Please check that the address in the config.json file is the one provided by the Lovense Connect app on your phone.");
            Console.ForegroundColor = defaultColor;
        }

    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = defaultColor;
    }
}