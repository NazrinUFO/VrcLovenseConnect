namespace VrcLovenseConnect.ToyManagers
{
    internal interface IToyManager : IDisposable
    {
        IEnumerable<string> ToyNames { get; }

        bool IsToyFound { get; }

        Task FindToy();

        Task Vibrate(string toyName, float haptics);

        Task Rotate(string toyName, float haptics);

        Task Pump(string toyName, float haptics);
    }
}
