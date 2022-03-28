using Newtonsoft.Json;
using VrcLovenseConnect;

// == PROGRAM ==
OscModule oscModule;
Task task;

ConsoleHelper.DefaultColor = Console.ForegroundColor;
ConsoleHelper.PrintInfo("VRCLovenseConnect (alpha)");

// Checks the configuration of the program
string configFile = File.ReadAllText("config.json");
Config? config = JsonConvert.DeserializeObject<Config>(configFile);

if (config == null)
{
    ConsoleHelper.PrintError("Error in configuration file. Please check its format.");
    ConsoleHelper.AwaitUserKeyPress();
    return;
}

if (!config.ControlParameters())
    return;

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
        ConsoleHelper.PrintError("Protocol error in configuration file. Please enter a valid protocol name.");
        ConsoleHelper.AwaitUserKeyPress();
        return;
}

using (toyManager)
{
    Console.WriteLine("Looking for a toy...");
    await toyManager.FindToy();

    if (toyManager.IsToyFound)
    {
        ConsoleHelper.PrintSuccess($"Toy found: {toyManager.ToyName}");
        ConsoleHelper.PrintInfo("INFO: Please turn off your toy before closing the program.");
    }
    else
    {
        if (config.Protocol == "Lovense")
            ConsoleHelper.PrintError("ERROR: Cannot find a toy! Please check the address in the config.json file and make sure your toy is connected to Lovense Connect.");
        else
            ConsoleHelper.PrintError("ERROR: Cannot find a toy! Please make sure Bluetooth is active and your toy is connected to your computer.");

        ConsoleHelper.AwaitUserKeyPress();
        return;
    }

    oscModule = new OscModule(config, toyManager);

    // Once connected, reads OSC messages.
    task = Task.Run(oscModule.Listen);

    // Any key press will close the program.
    ConsoleHelper.AwaitUserKeyPress();

    // Stops the program and waits for the loop to complete.
    oscModule.Play = false;
    task.Wait();

    // Stops vibration if started.
    if (oscModule.Haptics.HasValue && oscModule.Haptics.Value > 0)
    {
        toyManager.Vibrate(0).Wait();
    }
}
