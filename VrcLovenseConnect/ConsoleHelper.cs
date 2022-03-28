using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VrcLovenseConnect
{
    internal static class ConsoleHelper
    {
        internal static ConsoleColor DefaultColor { get; set; }

        /// <summary>
        /// Common process to wait for a user's input before closing the program.
        /// </summary>
        internal static void AwaitUserKeyPress()
        {
            Console.WriteLine("Press any key to close the program.");
            Console.ReadKey(true);
        }

        internal static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = DefaultColor;
        }

        internal static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = DefaultColor;
        }

        internal static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = DefaultColor;
        }
    }
}
