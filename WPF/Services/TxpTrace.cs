using System.Diagnostics;

namespace UI.Services
{
    internal static class TxpTrace
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message) =>
            Console.WriteLine(message);
    }
}
