namespace Aria2Manager.Core.Helpers
{
    public static class BitfieldParser
    {
        public static bool[] Parse(string bitfieldHex, long? numPieces)
        {
            if (string.IsNullOrWhiteSpace(bitfieldHex) || !numPieces.HasValue)
            {
                return new bool[0];
            }
            bool[] piecesStatus = new bool[numPieces.Value];
            int pieceIndex = 0;
            foreach (char hexChar in bitfieldHex)
            {
                //将十六进制字符转换为4位整数
                int val = Convert.ToInt32(hexChar.ToString(), 16);
                //依次读取4个bit
                for (int i = 3; i >= 0; i--)
                {
                    if (pieceIndex >= numPieces)
                    {
                        break; //忽略填充的多余位
                    }
                    //位运算判断该bit是否为1
                    piecesStatus[pieceIndex] = (val & (1 << i)) != 0;
                    pieceIndex++;
                }
            }
            return piecesStatus;
        }
    }
}
