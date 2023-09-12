namespace Aria2Manager.Utils
{
    public class Tools
    {
        //Byte转可读字符串
        static public string BytesToString(long byteCount)
        {
            long GBSize = 1024 * 1024 * 1024;
            long MBSize = 1024 * 1024;
            long KBSize = 1024;
            if (byteCount >= GBSize)
            {
                return ((double)(byteCount * 100 / GBSize) / 100).ToString() + "GB";
            }
            else if (byteCount >= MBSize)
            {
                return ((double)(byteCount * 100 / MBSize) / 100).ToString() + "MB";
            }
            else
            {
                return ((double)(byteCount * 100 / KBSize) / 100).ToString() + "KB";
            }
        }

        //秒（时间）转可读字符串
        static public string SecondsToString(long secCount)
        {
            string result = "";
            long HSize = 60 * 60;
            long MSize = 60;
            int h_num = (int)(secCount / HSize);
            result += h_num.ToString() + "h";
            secCount -= h_num * HSize;
            int m_num = (int)(secCount / MSize);
            result += m_num.ToString() + "m";
            secCount -= m_num * MSize;
            result += secCount.ToString() + "s";
            return result;
        }
    }
}
