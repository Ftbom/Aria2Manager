using Serilog;

namespace Aria2Manager.Core.Helpers
{
    public static class LogHelper
    {
        public static void Error(string message, Exception ex, bool throwException = true)
        {
            Log.Error($"{message}: {ex.Message}");
            if (throwException)
            {
                throw new Exception(message, ex);
            }
        }
        public static void Warning(string message, Exception ex, bool throwException = false)
        {
            Log.Warning($"{message}: {ex.Message}");
            if (throwException)
            {
                throw new Exception(message, ex);
            }
        }
        public static void Information(string message, Exception ex)
        {
            Log.Information($"{message}: {ex.Message}");
        }
        public static void Debug(string message, Exception ex)
        {
            Log.Debug($"{message}: {ex.Message}");
        }
    }
}
