using System.Diagnostics;

namespace BLL.Diagnostics
{
    internal static class TxpTrace
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message) =>
            Console.WriteLine(message);
    }
}
