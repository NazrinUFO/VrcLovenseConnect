namespace VrcLovenseConnect.ToyManagers
{
    internal interface IToyManager : IDisposable
    {
        string ToyName { get; }

        bool IsToyFound { get; }

        Task FindToy();

        Task Vibrate(float haptics);

        Task Rotate(float haptics);

        Task Pump(float haptics);
    }
}
