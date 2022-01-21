using System.Management;

namespace RainyFetch {
    class Program {
        private const string space = "     ";
        private static readonly string[] logo = {
            @"                                  ..,",
            @"                      ....,,:;+ccllll",
            @"        ...,,+:;  cllllllllllllllllll",
            @"  ,cclllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"                                     ",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  llllllllllllll  lllllllllllllllllll",
            @"  `'ccllllllllll  lllllllllllllllllll",
            @"        `' \\*::  :ccllllllllllllllll",
            @"                         ````''*::cll",
            @"                                   ``"
        };
        private static readonly ManagementClass mc = new();
        private static int intLineNum = 0;
        private static readonly Dictionary<string, string[]> dict = new() {
            { "Win32_ComputerSystem", new[] { "UserName", "Name", "Manufacturer", "SystemFamily" } },
            { "Win32_OperatingSystem", new[] { "Caption", "Version", "OSArchitecture", "LastBootUpTime", "LocalDateTime" } },
            { "Win32_BaseBoard", new[] { "Manufacturer", "Product", "SerialNumber", "Version" } },
            { "Win32_PhysicalMemory", new[] { "Capacity", "ConfiguredClockSpeed", "Manufacturer" } },
            { "Win32_DiskDrive", new[] { "Size", "Caption" } },
            { "Win32_NetworkAdapter", new[] { "PhysicalAdapter", "Name", "Speed" } }
        };
        private static readonly List<string[]> result = new();

        static void Main() {
            var count = 1;
            var order = "";
            var tab = string.Empty;

            result.Add(new[] { "\n", string.Empty });
            var dictCS = WMIC("Win32_ComputerSystem");
            var strUser = dictCS[0]["UserName"][(dictCS[0]["UserName"].IndexOf("\\") + 1)..].Trim();
            var strSystem = dictCS[0]["Name"];
            var strName = dictCS[0]["Manufacturer"] + " " + dictCS[0]["SystemFamily"];

            result.Add(new[] { strUser, "red" });
            result.Add(new[] { "@", "white" });
            result.Add(new[] { strSystem, "red" });
            result.Add(new[] { " · ", "white" });
            result.Add(new[] { strName, "red" });
            result.Add(new[] { "\n", string.Empty });

            var hr = string.Empty;
            for (int len = 0; len < strUser.Length + strSystem.Length + strName.Length + 4; len++) {
                hr += "-";
            }
            result.Add(new[] { hr, "white" });
            result.Add(new[] { "\n", string.Empty });

            var dictBB = WMIC("Win32_BaseBoard");
            result.Add(new[] { "M B: ", "red" });
            result.Add(new[] { dictBB[0]["Manufacturer"], "white" });
            if (dictBB[0]["Product"] != "None") {
                result.Add(new[] { " · ", "red" });
                result.Add(new[] { dictBB[0]["Product"], "white" });
            }
            if (dictBB[0]["Version"] != "None") {
                result.Add(new[] { " · ", "red" });
                result.Add(new[] { dictBB[0]["Version"], "white" });
            }
            result.Add(new[] { "\n", string.Empty });
            if (dictBB[0]["SerialNumber"] != "None") {
                result.Add(new[] { space + dictBB[0]["SerialNumber"], "white" });
                result.Add(new[] { "\n", string.Empty });
            }

            var dictOS = WMIC("Win32_OperatingSystem");
            result.Add(new[] { "O S: ", "red" });
            result.Add(new[] { dictOS[0]["Caption"], "white" });
            result.Add(new[] { "\n", string.Empty });
            result.Add(new[] { space + dictOS[0]["Version"], "white" });
            result.Add(new[] { " · ", "red" });
            result.Add(new[] { dictOS[0]["OSArchitecture"], "white" });
            result.Add(new[] { "\n", string.Empty });

            result.Add(new[] { "U P: ", "red" });
            Uptime(dictOS[0]["LastBootUpTime"], dictOS[0]["LocalDateTime"]);
            result.Add(new[] { "\n", string.Empty });

            result.Add(new[] { "CPU: ", "red" });
            var cpus = WMIC("Win32_Processor", "Name");
            count = 1;
            tab = string.Empty;
            foreach (var cpu in cpus) {
                if (cpus.Count > 1) {
                    order = count.ToString() + ". ";
                } else {
                    order = "";
                }
                if (count > 1) {
                    tab = space;
                }
                result.Add(new[] { tab + order, "red" });
                result.Add(new[] { cpu, "white" });
                result.Add(new[] { "\n", string.Empty });
                count++;
            }

            result.Add(new[] { "GPU: ", "red" });
            var gpus = WMIC("Win32_VideoController", "Name");
            count = 1;
            tab = string.Empty;
            foreach (var gpu in gpus) {
                if (gpus.Count > 1) {
                    order = count.ToString() + ". ";
                } else {
                    order = "";
                }
                if (count > 1) {
                    tab = space;
                }
                result.Add(new[] { tab + order, "red" });
                result.Add(new[] { gpu, "white" });
                result.Add(new[] { "\n", string.Empty });
                count++;
            }

            result.Add(new[] { "MEM: ", "red" });
            var mems = WMIC("Win32_PhysicalMemory");
            count = 1;
            tab = string.Empty;
            foreach (var mem in mems) {
                if (mems.Count > 1) {
                    order = count.ToString() + ". ";
                } else {
                    order = "";
                }
                if (count > 1) {
                    tab = space;
                }
                result.Add(new[] { tab + order, "red" });
                result.Add(new[] { CapcityCovertion(mem["Capacity"]), "white" });
                if (!string.IsNullOrEmpty(mem["ConfiguredClockSpeed"])) {
                    result.Add(new[] { " · ", "red" });
                    result.Add(new[] { mem["ConfiguredClockSpeed"] + " MHZ", "white" });
                }
                if (!string.IsNullOrEmpty(mem["Manufacturer"])) {
                    result.Add(new[] { " · ", "red" });
                    result.Add(new[] { mem["Manufacturer"], "white" });
                }
                result.Add(new[] { "\n", string.Empty });
                count++;
            }

            result.Add(new[] { "DSK: ", "red" });
            var dsks = WMIC("Win32_DiskDrive");
            var dskOn = new List<Dictionary<string, string>>();
            count = 1;
            tab = string.Empty;
            foreach (var dsk in dsks) {
                string size = CapcityCovertion(dsk["Size"]);
                if (!string.IsNullOrEmpty(size)) {
                    dskOn.Add(dsk);
                }
            }
            foreach (var dsk in dskOn) {
                if (dskOn.Count > 1) {
                    order = count.ToString() + ". ";
                } else {
                    order = "";
                }
                if (count > 1) {
                    tab = space;
                }
                result.Add(new[] { tab + order, "red" });
                result.Add(new[] { CapcityCovertion(dsk["Size"]), "white" });
                if (!string.IsNullOrEmpty(dsk["Caption"])) {
                    result.Add(new[] { " · ", "red" });
                    result.Add(new[] { dsk["Caption"], "white" });
                }
                result.Add(new[] { "\n", string.Empty });
                count++;
            }

            result.Add(new[] { "NIC: ", "red" });
            var nets = WMIC("Win32_NetworkAdapter");
            var netOn = new List<Dictionary<string, string>>();
            count = 1;
            tab = string.Empty;
            foreach (var net in nets) {
                if (Convert.ToBoolean(net["PhysicalAdapter"])) {
                    netOn.Add(net);
                }
            }
            foreach (var net in netOn) {
                if (netOn.Count > 1) {
                    order = count.ToString() + ". ";
                } else {
                    order = "";
                }
                if (count > 1) {
                    tab = space;
                }
                result.Add(new[] { tab + order, "red" });
                result.Add(new[] { net["Name"], "white" });
                result.Add(new[] { " · ", "red" });
                result.Add(new[] { SpeedCovertion(WMI(net["Name"])["LinkSpeed"]), "white" });
                result.Add(new[] { "\n", string.Empty });
                count++;
            }
            result.Add(new[] { "\n", string.Empty });

            ColorBlock();

            foreach (var line in result) {
                if (line[0] == "\n") {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    if (++intLineNum < logo.Length) {
                        Console.Write(logo[intLineNum] + "    ");
                    } else {
                        Console.Write("                                         ");
                    }
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), UpperFirstLetter(line[1]));
                    Console.Write(line[0]);
                    Console.ResetColor();
                }
            }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Blue;
            while (intLineNum < logo.Length) {
                Console.WriteLine(logo[intLineNum++]);
            }
            Console.ResetColor();

            Console.ReadKey();
        }

        private static List<Dictionary<string, string>> WMIC(string strObject) {
            List<Dictionary<string, string>> results = new();
            string[] properties = dict[strObject];
            mc.Path = new ManagementPath(strObject);
            mc.Options.UseAmendedQualifiers = false;
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc) {
                Dictionary<string, string> result = new();
                for (int i = 0; i < properties.Length; i++) {
                    result.Add(properties[i], "");
                    try {
                        string? query = mo.Properties[properties[i]].Value.ToString();
                        if (!string.IsNullOrEmpty(query)) {
                            result[properties[i]] = query;
                        }
                    } catch (Exception) {
                        continue;
                    }
                }
                results.Add(result);
            }
            return results;
        }

        private static List<string> WMIC(string strClass, string strObject) {
            mc.Path = new ManagementPath(strClass);
            mc.Options.UseAmendedQualifiers = true;
            ManagementObjectCollection moc = mc.GetInstances();
            List<string> result = new();
            foreach (ManagementObject mo in moc) {
                try {
                    var query = mo.Properties[strObject].Value.ToString();
                    if (!string.IsNullOrEmpty(query)) {
                        result.Add(query);
                    }
                } catch (Exception) {
                    continue;
                }
            }
            return result;
        }

        private static Dictionary<string, string> WMI(string name) {
            Dictionary<string, string> result = new() {
                { "LinkSpeed", "" }
            };
            SelectQuery selectQuery = new("SELECT * FROM MSNdis_LinkSpeed WHERE active=true");
            ManagementScope scope = new("root\\wmi");
            ManagementObjectSearcher searcher = new(scope, selectQuery);
            foreach (ManagementObject resultVal in searcher.Get()) {
                if (resultVal["InstanceName"].ToString() == name) {
                    result["LinkSpeed"] = resultVal["NdisLinkSpeed"].ToString()!;
                }
            }
            return result;
        }

        private static void ColorBlock() {
            ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            foreach (var color in colors) {
                result.Add(new[] { "██", color.ToString() });
            }
        }

        private static void Uptime(string strBoot, string strLocal) {
            DateTime boot = ManagementDateTimeConverter.ToDateTime(strBoot);
            DateTime local = ManagementDateTimeConverter.ToDateTime(strLocal);
            var uptime = local - boot;
            if (uptime.Days > 0) {
                result.Add(new[] { uptime.Days.ToString(), "white" });
                result.Add(new[] { " D ", "red" });
            }
            if (uptime.Hours > 0) {
                result.Add(new[] { uptime.Hours.ToString(), "white" });
                result.Add(new[] { " H ", "red" });
            }
            if (uptime.Minutes > 0) {
                result.Add(new[] { uptime.Minutes.ToString(), "white" });
                result.Add(new[] { " M ", "red" });
            }
            if (uptime.Seconds > 0) {
                result.Add(new[] { uptime.Seconds.ToString(), "white" });
                result.Add(new[] { " S ", "red" });
            }
        }

        public static string CapcityCovertion(string s) {
            var unit = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
            var unitIndex = 0;
            try {
                var d = double.Parse(s);
                if (string.IsNullOrEmpty(s)) {
                    return string.Empty;
                }
                while (d >= 1024) {
                    d /= (double)1024;
                    unitIndex++;
                }
                return Math.Round(d, 2).ToString() + " " + unit[unitIndex];
            } catch (Exception) {
                return string.Empty;
            }
        }

        public static string SpeedCovertion(string s) {
            var unit = new[] { "KB/s", "MB/s", "GB/s", "TB/s", "PB/s" };
            var unitIndex = 0;
            if (s == "9223372036854775807") {
                return "N/A";
            }
            try {
                var d = double.Parse(s);
                while (d >= 1000) {
                    d /= (double)1000;
                    unitIndex++;
                }
                return Math.Round(d, 2).ToString() + " " + unit[unitIndex];
            } catch (Exception) {
                return string.Empty;
            }
        }

        public static string UpperFirstLetter(string s) {
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s[1..];
        }
    }
}