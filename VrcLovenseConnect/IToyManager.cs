using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect
{
    internal interface IToyManager : IDisposable
    {
        string ToyName { get; }

        bool IsToyFound { get; }

        Task FindToy();

        Task Vibrate(float haptics);
    }
}
