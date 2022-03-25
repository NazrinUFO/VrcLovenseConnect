using Newtonsoft.Json;
using Rug.Osc;
using System.Net;
using VrcLovenseConnect;

/// <summary>
/// The default text color in console.
/// </summary>
ConsoleColor defaultColor = Console.ForegroundColor;

/// <summary>
/// Indicates whether the program keeps running.
/// </summary>
bool play = true;

/// <summary>
/// The intensity read from an OSC message.
/// </summary>
float? haptics = null;

/// <summary>
/// Number of messages read.
/// </summary>
int nbrMessages = 0;

/// <summary>
/// Number of counted retries.
/// </summary>
int retries = 0;

// == PROGRAM ==
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("VRCLovenseConnect (alpha)");
Console.ForegroundColor = defaultColor;

// Checks the configuration of the program
string configFile = File.ReadAllText("config.json");
Config? config = JsonConvert.DeserializeObject<Config>(configFile);

if (config == null)
{
    Console.WriteLine("Error in configuration file. Please check its format.");
    AwaitUserKeyPress();
    return;
}

if (config.OscPort <= 0)
{
    Console.WriteLine("Port error in configuration file. Pleaser enter a valid port number.");
    AwaitUserKeyPress();
    return;
}

if (config.Protocol == "Lovense" && string.IsNullOrWhiteSpace(config.Address))
{
    Console.WriteLine("Address error in configuration file. Please enter the address provided by the Lovense Connect app on your phone.");
    AwaitUserKeyPress();
    return;
}

if (config.Protocol == "Buttplug" && config.ScanTime <= 0)
{
    Console.WriteLine("Scan time error in configuration file. Pleaser enter a non-zero, positive value.");
    AwaitUserKeyPress();
    return;
}

if (config.Limit <= 0)
{
    Console.WriteLine("Limit error in configuration file. Pleaser enter a non-zero, positive value.");
    AwaitUserKeyPress();
    return;
}

if (string.IsNullOrWhiteSpace(config.Parameter))
{
    Console.WriteLine("Avatar Parameter error in configuration file. Please enter a valid parameter name.");
    AwaitUserKeyPress();
    return;
}

// Connection to toy
IToyManager toyManager;

switch (config.Protocol)
{
    case "Lovense":
        toyManager = new LovenseManager(config.Address);
        break;

    case "Buttplug":
        toyManager = new ButtplugManager(config.ScanTime);
        break;

    default:
        Console.WriteLine("Protocol error in configuration file. Please enter a valid protocol name.");
        AwaitUserKeyPress();
        return;
}

using (toyManager)
{
    Console.WriteLine("Looking for a toy...");
    await toyManager.FindToy();

    if (toyManager.IsToyFound)
    {
        Console.WriteLine($"Toy found: {toyManager.ToyName}");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        
        if (config.Protocol == "Lovense")
            Console.WriteLine("WARNING: Cannot find a toy! Please check the address in the config.json file and make sure your toy is connected to Lovense Connect.");
        else
            Console.WriteLine("WARNING: Cannot find a toy! Please make sure your toy is connected to your computer or within the same network.");

        Console.ForegroundColor = defaultColor;
        AwaitUserKeyPress();
        return;
    }

    // Once connected, starts a seperate thread to read OSC messages and leave the console available for inputs.
    var task = Task.Run(Listen);

    // Any key press will close the program.
    AwaitUserKeyPress();

    // Stops the program and waits for the loop to complete.
    play = false;
    task.Wait();

    // Stops vibration if started.
    if (haptics.HasValue && haptics.Value > 0)
    {
        toyManager.Vibrate(0).Wait();
    }
}

// == END PROGRAM ==

/// <summary>
/// Common process to wait for a user's input before closing the program.
/// </summary>
void AwaitUserKeyPress()
{
    Console.WriteLine("Press any key to close the program.");
    Console.ReadKey(true);
}

/// <summary>
/// Listens to OSC messages and updates toys.
/// </summary>
async void Listen()
{
    OscMessage? message;
    bool messageReceived;

    // Listens for OSC messages on localhost, port 9001.
    using var oscReceiver = new OscReceiver(IPAddress.Loopback, config.OscPort);
    oscReceiver.Connect();
    Console.WriteLine($"Connected and listening to {oscReceiver.LocalAddress}:{oscReceiver.RemoteEndPoint.Port}...");

    // Loops until the program is closed.
    while (play)
    {
        try
        {
            // Listens for one tick. Non-blocking.
            messageReceived = oscReceiver.TryReceive(out OscPacket packet);

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
                            // Updates the toy to the vibration value.
                            await toyManager.Vibrate(haptics.Value);
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
                    if (retries > config.Limit && haptics.HasValue && haptics.Value > 0)
                    {
                        haptics = 0;
                        await toyManager.Vibrate(haptics.Value);
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
}
