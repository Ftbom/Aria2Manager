namespace Aria2Manager.Core.Helpers
{
    public class BTPeerIdParser
    {
        public static string Parse(string peerId)
        {
            string decodedPeerId = DecodeToBytes(peerId);
            if (string.IsNullOrWhiteSpace(decodedPeerId)) { return string.Empty; }
            var parts = decodedPeerId.Split('-');
            if (parts[0].Length == 0 && parts.Length > 1)
            {
                return parts[1];
            }
            return parts[0];
        }
        private static string DecodeToBytes(string encodedStr)
        {
            string decoded = string.Empty;
            if (string.IsNullOrEmpty(encodedStr)) { return decoded; }
            for (int i = 0; i < encodedStr.Length; i++)
            {
                char c = encodedStr[i];
                if (c == '%' && i + 2 < encodedStr.Length)
                {
                    string hex = encodedStr.Substring(i + 1, 2);
                    try
                    {
                        decoded += (char)Convert.ToByte(hex, 16);
                        i += 2;
                    }
                    catch (FormatException)
                    {
                        decoded += c;
                    }
                }
                else
                {
                    decoded += c;
                }
            }
            return decoded;
        }
    }
}
