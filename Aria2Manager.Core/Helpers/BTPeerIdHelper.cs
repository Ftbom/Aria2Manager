//算法来源：https://github.com/mayswind/angular-bittorrent-peerid/blob/master/angular-bittorrent-peerid.js
//在AI的帮助下完成

namespace Aria2Manager.Core.Helpers
{
    public class PeerClientInfo
    {
        public string Client { get; set; }
        public string? Version { get; set; }
        public PeerClientInfo(string client, string? version = null)
        {
            Client = client;
            Version = version;
        }
        public override string ToString()
        {
            return Version == null ? Client : $"{Client} {Version}";
        }
    }
    public class BTPeerIdParser
    {
        private delegate string? VersionParser(string v);
        private static readonly Dictionary<string, string> AzStyleClients = new Dictionary<string, string>();
        private static readonly Dictionary<string, VersionParser> AzStyleClientVersions = new Dictionary<string, VersionParser>();
        private static readonly Dictionary<string, string> ShadowStyleClients = new Dictionary<string, string>();
        private static readonly Dictionary<string, VersionParser> ShadowStyleClientVersions = new Dictionary<string, VersionParser>();
        private static readonly Dictionary<string, string> MainlineStyleClients = new Dictionary<string, string>();
        private class CustomClient
        {
            public required string Id { get; set; }
            public required string Client { get; set; }
            public string? Version { get; set; }
            public int Position { get; set; } = 0;
        }
        private static readonly List<CustomClient> CustomStyleClients = new List<CustomClient>();
        private static readonly VersionParser VER_AZ_THREE_DIGITS = v => $"{v[0]}.{v[1]}.{v[2]}";
        private static readonly VersionParser VER_AZ_DELUGE = v =>
        {
            const string alphabet = "ABCDE";
            if (!char.IsDigit(v[2]))
            {
                return $"{v[0]}.{v[1]}.1{alphabet.IndexOf(v[2])}";
            }
            return $"{v[0]}.{v[1]}.{v[2]}";
        };
        private static readonly VersionParser VER_AZ_THREE_DIGITS_PLUS_MNEMONIC = v =>
        {
            string mnemonic = v[3].ToString();
            if (mnemonic == "B")
            {
                mnemonic = "Beta";
            }
            else if (mnemonic == "A")
            {
                mnemonic = "Alpha";
            }
            else
            {
                mnemonic = "";
            }
            return $"{v[0]}.{v[1]}.{v[2]} {mnemonic}";
        };
        private static readonly VersionParser VER_AZ_FOUR_DIGITS = v => $"{v[0]}.{v[1]}.{v[2]}.{v[3]}";
        private static readonly VersionParser VER_AZ_TWO_MAJ_TWO_MIN = v => $"{v[0]}{v[1]}.{v[2]}{v[3]}";
        private static readonly VersionParser VER_AZ_SKIP_FIRST_ONE_MAJ_TWO_MIN = v => $"{v[1]}.{v[2]}{v[3]}";
        private static readonly VersionParser VER_AZ_TRANSMISSION_STYLE = v =>
        {
            if (v[0] == '0' && v[1] == '0' && v[2] == '0')
            {
                return $"0.{v[3]}";
            }
            else if (v[0] == '0' && v[1] == '0')
            {
                return $"0.{v[2]}{v[3]}";
            }
            return $"{v[0]}.{v[1]}{v[2]}{(v[3] == 'Z' || v[3] == 'X' ? "+" : "")}";
        };
        private static readonly VersionParser VER_AZ_WEBTORRENT_STYLE = v =>
        {
            string version = v[0] == '0' ? $"{v[1]}." : $"{v[0]}{v[1]}.";
            version += v[2] == '0' ? v[3].ToString() : $"{v[2]}{v[3]}";
            return version;
        };
        private static readonly VersionParser VER_AZ_KTORRENT_STYLE = v => "1.2.3=[RD].4";
        private static readonly VersionParser VER_AZ_THREE_ALPHANUMERIC_DIGITS = v => "2.33.4";
        private static readonly VersionParser VER_NONE = v => null;
        static BTPeerIdParser()
        {
            //初始化AZ风格客户端
            AddAzStyle("A~", "Ares", VER_AZ_THREE_DIGITS);
            AddAzStyle("AG", "Ares", VER_AZ_THREE_DIGITS);
            AddAzStyle("AN", "Ares", VER_AZ_FOUR_DIGITS);
            AddAzStyle("AR", "Ares");
            AddAzStyle("AV", "Avicora");
            AddAzStyle("AX", "BitPump", VER_AZ_TWO_MAJ_TWO_MIN);
            AddAzStyle("AT", "Artemis");
            AddAzStyle("AZ", "Vuze", VER_AZ_FOUR_DIGITS);
            AddAzStyle("BB", "BitBuddy", v => "1.234");
            AddAzStyle("BC", "BitComet", VER_AZ_SKIP_FIRST_ONE_MAJ_TWO_MIN);
            AddAzStyle("BE", "BitTorrent SDK");
            AddAzStyle("BF", "BitFlu", VER_NONE);
            AddAzStyle("BG", "BTG", VER_AZ_FOUR_DIGITS);
            AddAzStyle("bk", "BitKitten (libtorrent)");
            AddAzStyle("BR", "BitRocket", v => "1.2(34)");
            AddAzStyle("BS", "BTSlave");
            AddAzStyle("BT", "BitTorrent", VER_AZ_THREE_DIGITS_PLUS_MNEMONIC);
            AddAzStyle("BW", "BitWombat");
            AddAzStyle("BX", "BittorrentX");
            AddAzStyle("CB", "Shareaza Plus");
            AddAzStyle("CD", "Enhanced CTorrent", VER_AZ_TWO_MAJ_TWO_MIN);
            AddAzStyle("CT", "CTorrent", v => "1.2.34");
            AddAzStyle("DP", "Propogate Data Client");
            AddAzStyle("DE", "Deluge", VER_AZ_DELUGE);
            AddAzStyle("EB", "EBit");
            AddAzStyle("ES", "Electric Sheep", VER_AZ_THREE_DIGITS);
            AddAzStyle("FC", "FileCroc");
            AddAzStyle("FG", "FlashGet", VER_AZ_SKIP_FIRST_ONE_MAJ_TWO_MIN);
            AddAzStyle("FX", "Freebox BitTorrent");
            AddAzStyle("FT", "FoxTorrent/RedSwoosh");
            AddAzStyle("GR", "GetRight", v => "1.2");
            AddAzStyle("GS", "GSTorrent");
            AddAzStyle("HL", "Halite", VER_AZ_THREE_DIGITS);
            AddAzStyle("HN", "Hydranode");
            AddAzStyle("KG", "KGet");
            AddAzStyle("KT", "KTorrent", VER_AZ_KTORRENT_STYLE);
            AddAzStyle("LC", "LeechCraft");
            AddAzStyle("LH", "LH-ABC");
            AddAzStyle("LK", "linkage", VER_AZ_THREE_DIGITS);
            AddAzStyle("LP", "Lphant", VER_AZ_TWO_MAJ_TWO_MIN);
            AddAzStyle("LT", "libtorrent (Rasterbar)", VER_AZ_THREE_ALPHANUMERIC_DIGITS);
            AddAzStyle("lt", "libTorrent (Rakshasa)", VER_AZ_THREE_ALPHANUMERIC_DIGITS);
            AddAzStyle("LW", "LimeWire", VER_NONE);
            AddAzStyle("MO", "MonoTorrent");
            AddAzStyle("MP", "MooPolice", VER_AZ_THREE_DIGITS);
            AddAzStyle("MR", "Miro");
            AddAzStyle("MT", "MoonlightTorrent");
            AddAzStyle("NE", "BT Next Evolution", VER_AZ_THREE_DIGITS);
            AddAzStyle("NX", "Net Transport");
            AddAzStyle("OS", "OneSwarm", VER_AZ_FOUR_DIGITS);
            AddAzStyle("OT", "OmegaTorrent");
            AddAzStyle("PC", "CacheLogic", v => "12.3-4");
            AddAzStyle("PT", "Popcorn Time");
            AddAzStyle("PD", "Pando");
            AddAzStyle("PE", "PeerProject");
            AddAzStyle("pX", "pHoeniX");
            AddAzStyle("qB", "qBittorrent", VER_AZ_DELUGE);
            AddAzStyle("QD", "qqdownload");
            AddAzStyle("RT", "Retriever");
            AddAzStyle("RZ", "RezTorrent");
            AddAzStyle("S~", "Shareaza alpha/beta");
            AddAzStyle("SB", "SwiftBit");
            AddAzStyle("SD", "迅雷在线 (Xunlei)");
            AddAzStyle("SG", "GS Torrent", VER_AZ_FOUR_DIGITS);
            AddAzStyle("SN", "ShareNET");
            AddAzStyle("SP", "BitSpirit", VER_AZ_THREE_DIGITS);
            AddAzStyle("SS", "SwarmScope");
            AddAzStyle("ST", "SymTorrent", v => "2.34");
            AddAzStyle("st", "SharkTorrent");
            AddAzStyle("SZ", "Shareaza");
            AddAzStyle("TG", "Torrent GO");
            AddAzStyle("TN", "Torrent.NET");
            AddAzStyle("TR", "Transmission", VER_AZ_TRANSMISSION_STYLE);
            AddAzStyle("TS", "TorrentStorm");
            AddAzStyle("TT", "TuoTu", VER_AZ_THREE_DIGITS);
            AddAzStyle("UL", "uLeecher!");
            AddAzStyle("UE", "µTorrent Embedded", VER_AZ_THREE_DIGITS_PLUS_MNEMONIC);
            AddAzStyle("UT", "µTorrent", VER_AZ_THREE_DIGITS_PLUS_MNEMONIC);
            AddAzStyle("UM", "µTorrent Mac", VER_AZ_THREE_DIGITS_PLUS_MNEMONIC);
            AddAzStyle("UW", "µTorrent Web", VER_AZ_THREE_DIGITS_PLUS_MNEMONIC);
            AddAzStyle("WD", "WebTorrent Desktop", VER_AZ_WEBTORRENT_STYLE);
            AddAzStyle("WT", "Bitlet");
            AddAzStyle("WW", "WebTorrent", VER_AZ_WEBTORRENT_STYLE);
            AddAzStyle("WY", "FireTorrent");
            AddAzStyle("VG", "哇嘎 (Vagaa)", VER_AZ_FOUR_DIGITS);
            AddAzStyle("XL", "迅雷在线 (Xunlei)");
            AddAzStyle("XT", "XanTorrent");
            AddAzStyle("XF", "Xfplay", VER_AZ_TRANSMISSION_STYLE);
            AddAzStyle("XX", "XTorrent", v => "1.2.34");
            AddAzStyle("XC", "XTorrent", v => "1.2.34");
            AddAzStyle("ZT", "ZipTorrent");
            AddAzStyle("7T", "aTorrent");
            AddAzStyle("ZO", "Zona", VER_AZ_FOUR_DIGITS);
            AddAzStyle("#@", "Invalid PeerID");
            //初始化Shadow风格客户端
            AddShadowStyle("A", "ABC");
            AddShadowStyle("O", "Osprey Permaseed");
            AddShadowStyle("Q", "BTQueue");
            AddShadowStyle("R", "Tribler");
            AddShadowStyle("S", "Shad0w");
            AddShadowStyle("T", "BitTornado");
            AddShadowStyle("U", "UPnP NAT");
            //初始化Mainline风格客户端
            AddMainlineStyle("M", "Mainline");
            AddMainlineStyle("Q", "Queen Bee");
            //初始化自定义/简单风格客户端
            AddSimpleClient("µTorrent", "1.7.0 RC", "-UT170-");
            AddSimpleClient("Azureus", "1", "Azureus");
            AddSimpleClient("Azureus", "2.0.3.2", "Azureus", 5);
            AddSimpleClient("Aria", "2", "-aria2-");
            AddSimpleClient("BitTorrent Plus!", "II", "PRC.P---");
            AddSimpleClient("BitTorrent Plus!", "P87.P---");
            AddSimpleClient("BitTorrent Plus!", "S587Plus");
            AddSimpleClient("BitTyrant (Azureus Mod)", "AZ2500BT");
            AddSimpleClient("Blizzard Downloader", "BLZ");
            AddSimpleClient("BTGetit", "BG", 10);
            AddSimpleClient("BTugaXP", "btuga");
            AddSimpleClient("BTugaXP", "BTuga", 5);
            AddSimpleClient("BTugaXP", "oernu");
            AddSimpleClient("Deadman Walking", "BTDWV-");
            AddSimpleClient("Deadman", "Deadman Walking-");
            AddSimpleClient("External Webseed", "Ext");
            AddSimpleClient("G3 Torrent", "-G3");
            AddSimpleClient("GreedBT", "2.7.1", "271-");
            AddSimpleClient("Hurricane Electric", "arclight");
            AddSimpleClient("HTTP Seed", "-WS");
            AddSimpleClient("JVtorrent", "10-------");
            AddSimpleClient("Limewire", "LIME");
            AddSimpleClient("Martini Man", "martini");
            AddSimpleClient("Pando", "Pando");
            AddSimpleClient("PeerApp", "PEERAPP");
            AddSimpleClient("SimpleBT", "btfans", 4);
            AddSimpleClient("Swarmy", "a00---0");
            AddSimpleClient("Swarmy", "a02---0");
            AddSimpleClient("Teeweety", "T00---0");
            AddSimpleClient("TorrentTopia", "346-");
            AddSimpleClient("XanTorrent", "DansClient");
            AddSimpleClient("MediaGet", "-MG1");
            AddSimpleClient("MediaGet", "2.1", "-MG21");
            AddSimpleClient("Amazon AWS S3", "S3-");
            AddSimpleClient("BitTorrent DNA", "DNA");
            AddSimpleClient("Opera", "OP");
            AddSimpleClient("Opera", "O");
            AddSimpleClient("Burst!", "Mbrst");
            AddSimpleClient("TurboBT", "turbobt");
            AddSimpleClient("BT Protocol Daemon", "btpd");
            AddSimpleClient("Plus!", "Plus");
            AddSimpleClient("XBT", "XBT");
            AddSimpleClient("BitsOnWheels", "-BOW");
            AddSimpleClient("eXeem", "eX");
            AddSimpleClient("MLdonkey", "-ML");
            AddSimpleClient("Bitlet", "BitLet");
            AddSimpleClient("AllPeers", "AP");
            AddSimpleClient("BTuga Revolution", "BTM");
            AddSimpleClient("Rufus", "RS", 2);
            AddSimpleClient("BitMagnet", "BM", 2);
            AddSimpleClient("QVOD", "QVOD");
            AddSimpleClient("Top-BT", "TB");
            AddSimpleClient("Tixati", "TIX");
            AddSimpleClient("folx", "-FL");
            AddSimpleClient("µTorrent Mac", "-UM");
            AddSimpleClient("µTorrent", "-UT");
        }
        private static void AddAzStyle(string id, string client, VersionParser? versionParser = null)
        {
            AzStyleClients[id] = client;
            AzStyleClientVersions[client] = versionParser ?? VER_AZ_FOUR_DIGITS;
        }
        private static void AddShadowStyle(string id, string client, VersionParser? versionParser = null)
        {
            ShadowStyleClients[id] = client;
            ShadowStyleClientVersions[client] = versionParser ?? VER_AZ_THREE_DIGITS;
        }
        private static void AddMainlineStyle(string id, string client)
        {
            MainlineStyleClients[id] = client;
        }
        private static void AddSimpleClient(string client, string version, string? id = null, int position = 0)
        {
            if (id == null)
            {
                CustomStyleClients.Add(new CustomClient
                {
                    Id = version,
                    Client = client,
                    Version = null,
                    Position = 0
                });
                return;
            }
            CustomStyleClients.Add(new CustomClient
            {
                Id = id,
                Client = client,
                Version = version,
                Position = position
            });
        }
        private static void AddSimpleClient(string client, string version, int id, int position = 0)
        {
            CustomStyleClients.Add(new CustomClient
            {
                Id = version,
                Client = client,
                Version = null,
                Position = id
            });
        }
        public static PeerClientInfo? Parse(string peerId)
        {
            string decodedPeerId = DecodeToBytes(peerId);
            if (string.IsNullOrWhiteSpace(decodedPeerId)) { return null; }
            if (IsPossibleSpoofClient(decodedPeerId))
            {
                var spirit = DecodeBitSpiritClient(decodedPeerId);
                if (spirit != null) { return spirit; }
                var comet = DecodeBitCometClient(decodedPeerId);
                if (comet != null) { return comet; }
                return new PeerClientInfo("BitSpirit?");
            }
            //AZ
            if (IsAzStyle(decodedPeerId))
            {
                string id = decodedPeerId.Substring(1, 2);
                if (AzStyleClients.TryGetValue(id, out string? clientName) && clientName != null)
                {
                    string? version = null;
                    if (AzStyleClientVersions.TryGetValue(clientName, out var vParser) && decodedPeerId.Length >= 7)
                    {
                        version = vParser(decodedPeerId.Substring(3, 4));
                    }
                    if (clientName.StartsWith("ZipTorrent") && decodedPeerId.Length >= 13 && decodedPeerId.Substring(8).StartsWith("bLAde"))
                    {
                        return new PeerClientInfo("Unknown [Fake: ZipTorrent]", version);
                    }
                    if (clientName == "µTorrent" && version == "6.0 Beta")
                    {
                        return new PeerClientInfo("Mainline", "6.0 Beta");
                    }
                    if (clientName.StartsWith("libTorrent (Rakshasa)"))
                    {
                        return new PeerClientInfo($"{clientName} / rTorrent*", version);
                    }
                    return new PeerClientInfo(clientName, version);
                }
            }
            //Shadow
            if (IsShadowStyle(decodedPeerId))
            {
                string id = decodedPeerId.Substring(0, 1);
                if (ShadowStyleClients.TryGetValue(id, out string? clientName) && clientName != null)
                {
                    return new PeerClientInfo(clientName); // TODO: shadow style 版本
                }
            }
            //Mainline
            if (IsMainlineStyle(decodedPeerId))
            {
                string id = decodedPeerId.Substring(0, 1);
                if (MainlineStyleClients.TryGetValue(id, out string? clientName) && clientName != null)
                {
                    return new PeerClientInfo(clientName); // TODO: mainline style 版本
                }
            }
            //再次检查BitSpirit/BitComet
            var spiritForce = DecodeBitSpiritClient(decodedPeerId);
            if (spiritForce != null) { return spiritForce; }
            var cometForce = DecodeBitCometClient(decodedPeerId);
            if (cometForce != null) { return cometForce; }
            //自定义/简单客户端
            var simpleClient = CustomStyleClients.FirstOrDefault(c =>
                decodedPeerId.Length >= c.Position + c.Id.Length &&
                decodedPeerId.Substring(c.Position, c.Id.Length) == c.Id);
            if (simpleClient != null)
            {
                return new PeerClientInfo(simpleClient.Client, simpleClient.Version);
            }
            //非标准客户端
            var awkwardClient = IdentifyAwkwardClient(decodedPeerId);
            if (awkwardClient != null)
            {
                return awkwardClient;
            }
            return new PeerClientInfo("unknown");
        }
        private static bool IsAzStyle(string peerId)
        {
            if (peerId.Length < 8 || peerId[0] != '-') { return false; }
            if (peerId[7] == '-') { return true; }

            string sub = peerId.Substring(1, 2);
            string[] hacks = { "FG", "LH", "NE", "KT", "SP" };
            return hacks.Contains(sub);
        }
        private static bool IsShadowStyle(string peerId)
        {
            if (peerId.Length < 6 || peerId[5] != '-') { return false; }
            if (!char.IsLetter(peerId[0])) { return false; }
            if (!(char.IsDigit(peerId[1]) || peerId[1] == '-')) { return false; }

            int lastVersionNumberIndex = 4;
            for (; lastVersionNumberIndex > 0; lastVersionNumberIndex--)
            {
                if (peerId[lastVersionNumberIndex] != '-')
                {
                    break;
                }
            }

            for (int i = 1; i <= lastVersionNumberIndex; i++)
            {
                char c = peerId[i];
                if (c == '-') { return false; }
                if (!char.IsLetterOrDigit(c) && c != '.') { return false; }
            }
            return true;
        }
        private static bool IsMainlineStyle(string peerId)
        {
            if (peerId.Length < 8) { return false; }
            return peerId[2] == '-' && peerId[7] == '-' && (peerId[4] == '-' || peerId[5] == '-');
        }
        private static bool IsPossibleSpoofClient(string peerId)
        {
            return peerId.EndsWith("UDP0") || peerId.EndsWith("HTTPBT");
        }
        private static PeerClientInfo? DecodeBitSpiritClient(string peerId)
        {
            if (peerId.Length < 4 || peerId.Substring(2, 2) != "BS") { return null; }
            string version = ((int)peerId[1]).ToString();
            if (version == "0") { version = "1"; }
            return new PeerClientInfo("BitSpirit", version);
        }
        private static PeerClientInfo? DecodeBitCometClient(string peerId)
        {
            if (peerId.Length < 10) { return null; }
            string modName = "";
            if (peerId.StartsWith("exbc"))
            {
                modName = "";
            }
            else if (peerId.StartsWith("FUTB"))
            {
                modName = "(Solidox Mod)";
            }
            else if (peerId.StartsWith("xUTB"))
            {
                modName = "(Mod 2)";
            }
            else
            {
                return null;
            }
            bool isBitlord = peerId.Substring(6, 4) == "LORD";
            string clientName = isBitlord ? "BitLord" : "BitComet";
            string majVersion = ((int)peerId[4]).ToString();
            int minVersionLength = (isBitlord && majVersion != "0") ? 1 : 2;
            string minVersion = ((int)peerId[5]).ToString().PadLeft(minVersionLength, '0');
            return new PeerClientInfo($"{clientName}{(string.IsNullOrEmpty(modName) ? "" : " " + modName)}", $"{majVersion}.{minVersion}");
        }
        private static PeerClientInfo? IdentifyAwkwardClient(string peerId)
        {
            if (peerId.Length < 20) { return null; }
            int firstNonZeroIndex = 20;
            for (int i = 0; i < 20; i++)
            {
                if (peerId[i] > 0)
                {
                    firstNonZeroIndex = i;
                    break;
                }
            }
            //Shareaza check
            if (firstNonZeroIndex == 0)
            {
                bool isShareaza = true;
                for (int i = 0; i < 16; i++)
                {
                    if (peerId[i] == 0)
                    {
                        isShareaza = false;
                        break;
                    }
                }
                if (isShareaza)
                {
                    for (int i = 16; i < 20; i++)
                    {
                        if (peerId[i] != (peerId[i % 16] ^ peerId[15 - (i % 16)]))
                        {
                            isShareaza = false;
                            break;
                        }
                    }
                    if (isShareaza) return new PeerClientInfo("Shareaza");
                }
            }
            if (firstNonZeroIndex == 9 && peerId[9] == 3 && peerId[10] == 3 && peerId[11] == 3)
            {
                return new PeerClientInfo("I2PSnark");
            }
            if (firstNonZeroIndex == 12 && peerId[12] == 97 && peerId[13] == 97)
            {
                return new PeerClientInfo("Experimental", "3.2.1b2");
            }
            if (firstNonZeroIndex == 12 && peerId[12] == 0 && peerId[13] == 0)
            {
                return new PeerClientInfo("Experimental", "3.1");
            }
            if (firstNonZeroIndex == 12)
            {
                return new PeerClientInfo("Mainline");
            }
            return null;
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