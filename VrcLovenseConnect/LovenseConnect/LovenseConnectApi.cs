using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

/// <summary>
/// The Namespace Of The Lovense Connect API; Created By Plague.
/// </summary>

namespace LovenseConnect
{
    /// <summary>
    /// The Main Class Of The Lovense Connect API; Created By Plague.
    /// </summary>
    [Obfuscation(Exclude = true, ApplyToMembers = true, StripAfterObfuscation = true)]
    public class LovenseConnectApi
    {
        /// <summary>
        /// Information About The Lovense Toy, Held In A Convenient Package.
        /// </summary>
        public class LovenseToy
        {
            #region Debug Info
            public string FVersion { get; set; } = "0";
            public string Version { get; set; } = "";
            #endregion

            #region Useful Info
            public string Name { get; set; } = "Unknown";

            public string Battery { get; set; } = "0%";
            public bool Status { get; set; } = false;

            public string Id { get; set; } = "";

            public string NickName { get; set; } = "";
            #endregion
        }

        /// <summary>
        /// The Local WebClient To Send Requests.
        /// </summary>
        private static HttpClient Client { get; set; } = new HttpClient();

        /// <summary>
        /// Toys Caching For Lovense To Function Efficiently.
        /// </summary>
        public static List<LovenseToy> Toys { get; set; } = new List<LovenseToy>();

        /// <summary>
        /// Gets A List Of Toys Connected To This Local Lovense Connect Server URL.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <returns>A List Of Toys Connected To This Local Lovense Connect Server URL.</returns>
        public static async Task<List<LovenseToy>?> GetToys(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            IsRequestPending = true;

            using var packet = await Client.GetAsync(new Uri(url.ToLower().Replace("/gettoys", "") + "/GetToys"));
            using var content = packet.Content;
            var response = await content.ReadAsStringAsync();

            IsRequestPending = false;

            if (!response.ToLower().Contains("\"ok\""))
            {
                return null;
            }

            if (response == "{}")
            {
                return null;
            }

            JObject jsonData = JObject.Parse(response);

            if (jsonData?.GetValue("code")?.ToString() != "200" || jsonData?.GetValue("type")?.ToString().ToLower() != "ok")
            {
                return null;
            }

            List<LovenseToy> toys = new();

            JObject data = JObject.Parse(jsonData?.GetValue("data")?.ToString() ?? string.Empty);

            foreach (JProperty toyid in data.Properties())
            {
                LovenseToy? toy = JsonConvert.DeserializeObject<LovenseToy>(toyid.Value.ToString());
                
                if (toy != null)
                    toys.Add(toy);
            }

            Toys = toys;

            return toys;
        }

        /// <summary>
        /// Gets A Instance Of LovenseToy Which Contains Info About The Toy From The URL And ID.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <param name="id">The Toy ID - Fun Fact: This Is The Device's MAC Address.</param>
        /// <returns>The LovenseToy Instance Containing Info About The Toy, Such As Battery Percentage, Type, Etc.</returns>
        public static async Task<LovenseToy?> GetToyInfoFromID(string url, string id)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(id))
            {
                return null;
            }

            return (await GetToys(url))?.FirstOrDefault(o => o.Id == id);
        }

        /// <summary>
        /// The Current Lovense Amount Of All Toy IDs.
        /// </summary>
        private static Dictionary<string, int> CurrentLovenseAmount = new Dictionary<string, int>();

        /// <summary>
        /// Is A Lovense Request In Progress?
        /// </summary>
        public static bool IsRequestPending { get; set; } = false;

        /// <summary>
        /// The Last Known Latency Recorded.
        /// </summary>
        public static int LastKnownLatency { get; set; } = 225;

        /// <summary>
        /// Stopwatch Used For Calculating Latency.
        /// </summary>
        public static Stopwatch DelayWatch { get; set; } = new Stopwatch();

        /// <summary>
        /// A Simple "Vibrate This Much" Method Which Will Vibrate The Toy {x} Amount From The Local Lovense Connect Server URL And ID Of The Toy.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <param name="id">The Toy ID - Fun Fact: This Is The Device's MAC Address.</param>
        /// <param name="amount">The Vibration Intensity.</param>
        /// <param name="ignoreDuplicateRequests">Whether To Ignore Duplicate Requests Or Not.</param>
        /// <returns></returns>
        public static async Task<bool> VibrateToy(string url, string id, int amount, bool ignoreDuplicateRequests = false)
        {
            try
            {
                if (!InitCommand(id, amount, ignoreDuplicateRequests, "Vibrate", out var toy))
                    return false;

                using var packet = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate?v=" + amount + "&t=" + toy?.Id ?? string.Empty);
                using var content = packet.Content;
                var response = await content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                {
                    IsRequestPending = false;

                    return false;
                }

                if (toy?.Name.ToLower().Contains("edge") ?? false)
                {
                    await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate1?v=" + amount + "&t=" + toy.Id);

                    using var vibrate2Packet = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate2?v=" + amount + "&t=" + toy.Id);
                    using var vibrate2Content = vibrate2Packet.Content;
                    response = await vibrate2Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                    {
                        IsRequestPending = false;

                        return false;
                    }
                }
                else
                {
                    using var vibratePacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate?v=" + amount + "&t=" + toy?.Id);
                    using var vibrateContent = vibratePacket.Content;
                    response = await vibrateContent.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                    {
                        IsRequestPending = false;

                        return false;
                    }
                }

                return EndCommand("Vibrate", amount, response);
            }
            catch
            {
                IsRequestPending = false;

                return false;
            }
        }

        /// <summary>
        /// A Simple "Rotate This Much" Method Which Will Rotate The Toy {x} Amount From The Local Lovense Connect Server URL And ID Of The Toy.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <param name="id">The Toy ID - Fun Fact: This Is The Device's MAC Address.</param>
        /// <param name="amount">The Rotation Intensity.</param>
        /// <param name="ignoreDuplicateRequests">Whether To Ignore Duplicate Requests Or Not.</param>
        /// <returns></returns>
        public static async Task<bool> RotateToy(string url, string id, int amount, bool ignoreDuplicateRequests = false)
        {
            try
            {
                if (!InitCommand(id, amount, ignoreDuplicateRequests, "Rotate", out var toy))
                    return false;

                using var rotatePacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Rotate?v=" + amount + "&t=" + toy?.Id);
                using var rotateContent = rotatePacket.Content;
                var response = await rotateContent.ReadAsStringAsync();

                return EndCommand("Rotate", amount, response);
            }
            catch
            {
                IsRequestPending = false;

                return false;
            }
        }

        /// <summary>
        /// A Simple "Air Contract" Method Which Will Pump The Toy {x} Amount From The Local Lovense Connect Server URL And ID Of The Toy.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <param name="id">The Toy ID - Fun Fact: This Is The Device's MAC Address.</param>
        /// <param name="amount">The Pumping Intensity.</param>
        /// <param name="ignoreDuplicateRequests">Whether To Ignore Duplicate Requests Or Not.</param>
        /// <returns></returns>
        public static async Task<bool> PumpToy(string url, string id, int amount, bool ignoreDuplicateRequests = false)
        {
            try
            {
                if (!InitCommand(id, amount, ignoreDuplicateRequests, "AirAuto", out var toy))
                    return false;

                using var airAutoPacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/AirAuto?v=" + amount + "&t=" + toy?.Id);
                using var airAutoContent = airAutoPacket.Content;
                var response = await airAutoContent.ReadAsStringAsync();

                return EndCommand("AirAuto", amount, response);
            }
            catch
            {
                IsRequestPending = false;

                return false;
            }
        }

        private static bool InitCommand(string id, int amount, bool ignoreDuplicateRequests, string command, out LovenseToy? toy)
        {
            toy = null;

            if (ignoreDuplicateRequests && CurrentLovenseAmount != null && CurrentLovenseAmount.ContainsKey(command) && CurrentLovenseAmount[command] == amount)
            {
                return false;
            }

            if (IsRequestPending)
            {
                return false;
            }

            DelayWatch.Reset();

            if (Toys == null || Toys.Count == 0)
            {
                return false;
            }

            toy = Toys?.Find(o => o.Id == id);

            IsRequestPending = true;

            if (!DelayWatch.IsRunning)
            {
                DelayWatch.Start();
            }

            if (toy?.Name == "Unknown")
            {
                IsRequestPending = false;
                return false; // Assume Toy Disconnected
            }

            return true;
        }

        private static bool EndCommand(string command, int amount, string response)
        {
            if (DelayWatch.IsRunning)
            {
                DelayWatch.Stop();

                LastKnownLatency = (int)DelayWatch.Elapsed.TotalMilliseconds;
            }

            IsRequestPending = false;

            if (!string.IsNullOrEmpty(response) && response.ToLower().Contains("\"ok\""))
            {
                if (CurrentLovenseAmount != null)
                {
                    CurrentLovenseAmount[command] = amount;
                }

                IsRequestPending = false;

                return true;
            }

            IsRequestPending = false;

            return false;
        }

        #region No plan to implement this.
        /// <summary>
        /// A Simple "Vibrate This Much" Method Which Will Vibrate The Toy {x} Amount; Iterating Through The Amounts List Per The Delay From The Local Lovense Connect Server URL And ID Of The Toy.
        /// </summary>
        /// <param name="url">The Local Lovense Connect Server URL.</param>
        /// <param name="id">The Toy ID - Fun Fact: This Is The Device's MAC Address.</param>
        /// <param name="amounts">The Vibration Intensitys.</param>
        /// <param name="DelayBetweenAmountsMilliseconds">The Delay Between Lovenses Defined In The Amounts.</param>
        /// <returns></returns>
        public static async Task<bool> VibrateToyWithPattern(string url, string id, List<int> amounts, float DelayBetweenAmountsMilliseconds = 200)
        {
            try
            {
                if (IsRequestPending)
                {
                    return false;
                }

                if (Toys == null || Toys.Count == 0)
                {
                    return false;
                }

                IsRequestPending = true;

                var toy = Toys.Find(o => o.Id == id);

                foreach (int amount in amounts)
                {
                    DelayWatch.Reset();

                    using var packet = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate?v=" + amount + "&t=" + toy?.Id ?? string.Empty);
                    using var content = packet.Content;
                    var response = await content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                    {
                        IsRequestPending = false;

                        return false;
                    }

                    if (toy?.Name.ToLower().Contains("edge") ?? false)
                    {
                        await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate1?v=" + amount + "&t=" + toy.Id);

                        using var vibrate2Packet = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate2?v=" + amount + "&t=" + toy.Id);
                        using var vibrate2Content = vibrate2Packet.Content;
                        response = await vibrate2Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                        {
                            IsRequestPending = false;

                            return false;
                        }
                    }
                    else
                    {
                        using var vibratePacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Vibrate?v=" + amount + "&t=" + toy?.Id);
                        using var vibrateContent = vibratePacket.Content;
                        response = await vibrateContent.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                        {
                            IsRequestPending = false;

                            return false;
                        }
                    }

                    if (toy?.Name.ToLower().Contains("max") ?? false)
                    {
                        using var airAutoPacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/AirAuto?v=" + RangeConv(amount, 0, 20, 0, 3) + "&t=" + toy.Id);
                        using var airAutoContent = airAutoPacket.Content;
                        response = await airAutoContent.ReadAsStringAsync();
                    }
                    else if (toy?.Name.ToLower().Contains("nora") ?? false)
                    {
                        using var rotatePacket = await Client.GetAsync(url.ToLower().Replace("/gettoys", "") + "/Rotate?v=" + amount + "&t=" + toy.Id);
                        using var rotateContent = rotatePacket.Content;
                        response = await rotateContent.ReadAsStringAsync();
                    }

                    if (DelayWatch.IsRunning)
                    {
                        DelayWatch.Stop();

                        LastKnownLatency = (int)DelayWatch.Elapsed.TotalMilliseconds;
                    }

                    if (!string.IsNullOrEmpty(response) && !response.ToLower().Contains("\"ok\""))
                    {
                        IsRequestPending = false;

                        return false;
                    }

                    CurrentLovenseAmount[id] = amount;

                    if (amounts.Count > 1)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(DelayBetweenAmountsMilliseconds));
                    }
                }

                IsRequestPending = false;

                return true;
            }
            catch
            {
                IsRequestPending = false;

                return false;
            }
        }
        #endregion

        public static int RangeConv(float input, float MinPossibleInput, float MaxPossibleInput, float MinConv, float MaxConv)
        {
            return Convert.ToInt32((((input - MinPossibleInput) * (MaxConv - MinConv)) / (MaxPossibleInput - MinPossibleInput)) + MinConv);
        }
    }
}