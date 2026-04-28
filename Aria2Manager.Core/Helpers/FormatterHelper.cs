namespace Aria2Manager.Core.Helpers
{
    public static class FormatterHelper
    {
        private const long GB = 1024 * 1024 * 1024;
        private const long MB = 1024 * 1024;
        private const long KB = 1024;
        //字节转可读字符串
        public static string BytesToString(long byteCount)
        {
            if (byteCount >= GB)
                return $"{(double)byteCount / GB:F2} GB";
            if (byteCount >= MB)
                return $"{(double)byteCount / MB:F2} MB";
            if (byteCount >= KB)
                return $"{(double)byteCount / KB:F2} KB";
            return $"{byteCount} B";
        }
        //秒数转时间字符串 (hh:mm:ss)
        public static string SecondsToString(long secCount)
        {
            //处理异常值
            if (secCount < 0) return "00:00:00";
            if (secCount >= 359999) return "99h 59m 59s"; //封顶处理
            var t = TimeSpan.FromSeconds(secCount);
            if (t.TotalHours >= 1)
            {
                return $"{(int)t.TotalHours}h {t.Minutes}m {t.Seconds}s";
            }
            return $"{t.Minutes}m {t.Seconds}s";
        }
    }
}
