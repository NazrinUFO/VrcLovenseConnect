using Newtonsoft.Json;
using VrcLovenseConnect.Helpers;
using VrcLovenseConnect.ToyManagers;

// == PROGRAM ==
OscModule oscModule;
Task task;

ConsoleHelper.DefaultColor = Console.ForegroundColor;
ConsoleHelper.PrintInfo("VRCLovenseConnect (beta)");

// Initiates logging
Logger.OpenLog("log.txt");

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
using IToyManager lovenseManager = new LovenseManager(config.Address);
using IToyManager buttplugManager = new ButtplugManager(config.RuntimeScanTime, (uint)config.Limit);

Console.WriteLine("Scanning toys through Bluetooth...");
await buttplugManager.FindToy();
Console.WriteLine("Scanning toys through Lovense Connect...");
await lovenseManager.FindToy();

if (lovenseManager.IsToyFound || buttplugManager.IsToyFound)
{
    try
    {
        // Saves detected settings to file.
        SaveToys(lovenseManager, "Lovense");
        SaveToys(buttplugManager, "Buttplug");
        File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
    }
    catch (Exception ex)
    {
        Logger.LogException(ex);
        return;
    }

    ConsoleHelper.PrintInfo("INFO: Although an attempt to stop the toys will be happening before closing the program, you may still have to manually turn them off.");
}
else
{
    ConsoleHelper.PrintError("ERROR: Cannot find a toy. Please make sure Bluetooth is active and your toy is connected to your computer.");
    ConsoleHelper.PrintError("For detection through Lovense Connect, check the address in the config.json file and make sure your toy is connected to Lovense Connect on your phone.");
    ConsoleHelper.AwaitUserKeyPress();
    return;
}

oscModule = new OscModule(config, lovenseManager, buttplugManager);

// Once connected, reads OSC messages.
task = Task.Run(oscModule.Listen);

// Any key press will close the program.
ConsoleHelper.AwaitUserKeyPress();

// Stops the program and waits for the loop to complete.
oscModule.Play = false;
task.Wait();

// Stops all toys.
foreach (var toy in config.Toys)
    oscModule.StopToy(toy).Wait();

// Ends logging
Logger.CloseLog();

void SaveToys(IToyManager toyManager, string protocol)
{
    foreach (var toyName in toyManager.ToyNames)
    {
        ConsoleHelper.PrintSuccess($"Toy found: {toyName} ({protocol})");

        if (!config.Toys.Exists(toy => toy.Name == toyName))
        {
            // Searches for an empty toy setting and overrides it.
            try
            {
                var toy = config.Toys.First(toy => string.IsNullOrWhiteSpace(toy.Name));
                toy.Name = toyName;
                toy.Protocol = protocol;
            }
            catch
            {
                ConsoleHelper.PrintError("No empty space to save the detected toy. Please make sure to have an empty slot by leaving a name and protocol blank.");
                ConsoleHelper.AwaitUserKeyPress();
                throw;
            }
        }
    }
}