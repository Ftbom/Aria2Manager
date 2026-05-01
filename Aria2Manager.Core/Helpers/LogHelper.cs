using Serilog;

namespace Aria2Manager.Core.Helpers
{
    public static class LogHelper
    {
        public static void Error(string message, Exception? ex = null, bool throwException = true)
        {
            Log.Error(ex, message);
            if (throwException && ex != null)
            {
                throw new Exception(message, ex);
            }
        }
        public static void Warning(string message, Exception? ex = null, bool throwException = false)
        {
            Log.Warning(ex, message);
            if (throwException && ex != null)
            {
                throw new Exception(message, ex);
            }
        }
        public static void Information(string message)
        {
            Log.Information(message);
        }
        public static void Debug(string message, Exception? ex = null)
        {
            Log.Warning(ex, message);
        }
    }
}
