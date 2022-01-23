using System.Globalization;
using System.Management;

namespace RainyFetch;

internal static class Program {
    private const string Space = "     ";

    private static readonly string[] Logo = {
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

    private static readonly string[] Architecture = {
        "x86",
        "MIPS",
        "Alpha",
        "PowerPC",
        "",
        "ARM",
        "ia64",
        // Itanium-based systems
        "",
        "",
        "x64"
    };

    private static readonly ManagementClass Mc = new();
    private static int _lineNum;

    private static readonly Dictionary<string, string[]> Dict = new() {
        {"Win32_ComputerSystem", new[] {"UserName", "Name", "Manufacturer", "SystemFamily"}},
        {"Win32_OperatingSystem", new[] {"Caption", "Version", "OSArchitecture", "LastBootUpTime", "LocalDateTime", "RegisteredUser"}},
        {"Win32_Processor", new[] {"Name", "CurrentClockSpeed", "MaxClockSpeed", "NumberOfCores", "NumberOfEnabledCore", "NumberOfLogicalProcessors", "ThreadCount", "L2CacheSize", "L3CacheSize", "Architecture"}},
        {"Win32_VideoController", new[] {"Name", "AdapterRAM", "AdapterDACType"}},
        {"Win32_BaseBoard", new[] {"Manufacturer", "Product", "SerialNumber", "Version"}},
        {"Win32_PhysicalMemory", new[] {"Capacity", "ConfiguredClockSpeed", "Manufacturer", "DeviceLocator"}},
        {"Win32_DiskDrive", new[] {"Size", "Caption"}},
        {"Win32_NetworkAdapter", new[] {"PhysicalAdapter", "Name", "Speed"}}
    };

    private static readonly List<string[]> Result = new();

    private static void Main() {
        string order;

        Result.Add(new[] {"\n", string.Empty});
        var dictCs = Wmic("Win32_ComputerSystem");
        var strUser = dictCs[0]["UserName"][(dictCs[0]["UserName"].IndexOf("\\", StringComparison.Ordinal) + 1)..]
            .Trim();
        var strSystem = dictCs[0]["Name"];
        var strName = dictCs[0]["Manufacturer"] + " " + dictCs[0]["SystemFamily"];

        Result.Add(new[] {strUser, "red"});
        Result.Add(new[] {"@", "white"});
        Result.Add(new[] {strSystem, "red"});
        Result.Add(new[] {" · ", "white"});
        Result.Add(new[] {strName, "red"});
        Result.Add(new[] {"\n", string.Empty});

        var hr = string.Empty;
        for (var len = 0; len < strUser.Length + strSystem.Length + strName.Length + 4; len++) hr += "-";
        Result.Add(new[] {hr, "white"});
        Result.Add(new[] {"\n", string.Empty});

        var dictBb = Wmic("Win32_BaseBoard");
        Result.Add(new[] {"M B: ", "red"});
        Result.Add(new[] {dictBb[0]["Manufacturer"], "white"});
        if (dictBb[0]["Product"] != "None") {
            Result.Add(new[] {" ", "red"});
            Result.Add(new[] {dictBb[0]["Product"], "white"});
        }

        if (dictBb[0]["Version"] != "None") {
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {dictBb[0]["Version"], "white"});
        }

        Result.Add(new[] {"\n", string.Empty});
        if (dictBb[0]["SerialNumber"] != "None") {
            Result.Add(new[] {Space + "SN: ", "red"});
            Result.Add(new[] {dictBb[0]["SerialNumber"], "white"});
            Result.Add(new[] {"\n", string.Empty});
        }

        var dictOs = Wmic("Win32_OperatingSystem");
        Result.Add(new[] {"O S: ", "red"});
        Result.Add(new[] {dictOs[0]["Caption"], "white"});
        Result.Add(new[] {" · ", "red"});
        Result.Add(new[] {dictOs[0]["Version"], "white"});
        Result.Add(new[] {"\n", string.Empty});
        Result.Add(new[] {Space + "Registered to ", "red"});
        Result.Add(new[] {dictOs[0]["RegisteredUser"], "white"});
        Result.Add(new[] {"\n", string.Empty});

        Result.Add(new[] {"U P: ", "red"});
        Uptime(dictOs[0]["LastBootUpTime"], dictOs[0]["LocalDateTime"]);
        Result.Add(new[] {"\n", string.Empty});

        Result.Add(new[] {"CPU: ", "red"});
        var cpus = Wmic("Win32_Processor");
        var count = 1;
        var tab = string.Empty;
        var strCore = "cores";
        var strThread = "threads";
        foreach (var cpu in cpus) {
            if (cpus.Count > 1)
                order = count + ". ";
            else
                order = string.Empty;
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {cpu["Name"], "white"});
            Result.Add(new[] {"\n", string.Empty});
            Result.Add(new[] {Space + cpu["CurrentClockSpeed"], "white"});
            Result.Add(new[] {" MHz", "red"});
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {"Max ", "red"});
            Result.Add(new[] {cpu["MaxClockSpeed"], "white"});
            Result.Add(new[] {" MHz", "red"});
            if (cpu["NumberOfEnabledCore"] == "1") strCore = "core";
            if (cpu["NumberOfCores"] == cpu["NumberOfEnabledCore"]) {
                Result.Add(new[] {" · " + cpu["NumberOfCores"], "white"});
                Result.Add(new[] {" " + strCore + " ", "red"});
            }
            else {
                Result.Add(new[] {Space + cpu["NumberOfCores"], "white"});
                Result.Add(new[] {"/", "red"});
                Result.Add(new[] {cpu["NumberOfEnabledCore"], "white"});
                Result.Add(new[] {" " + strCore + " ", "red"});
            }

            if (cpu["ThreadCount"] == "1") strThread = "thread";
            if (cpu["NumberOfLogicalProcessors"] == cpu["ThreadCount"]) {
                Result.Add(new[] {cpu["ThreadCount"], "white"});
                Result.Add(new[] {" " + strThread + " ", "red"});
            }
            else {
                Result.Add(new[] {cpu["NumberOfLogicalProcessors"], "white"});
                Result.Add(new[] {"/", "red"});
                Result.Add(new[] {cpu["ThreadCount"], "white"});
                Result.Add(new[] {" " + strThread + " ", "red"});
            }

            Result.Add(new[] {"\n", string.Empty});
            Result.Add(new[] {Space + "Architecture: ", "red"});
            Result.Add(new[] {Architecture[int.Parse(cpu["Architecture"])], "white"});
            Result.Add(new[] {" · L2: ", "red"});
            Result.Add(new[] {CapcityCovertion(cpu["L2CacheSize"])[0], "white"});
            Result.Add(new[] {" " + CapcityCovertion(cpu["L2CacheSize"], 1)[1], "red"});
            Result.Add(new[] {" · L3: ", "red"});
            Result.Add(new[] {CapcityCovertion(cpu["L3CacheSize"])[0], "white"});
            Result.Add(new[] {" " + CapcityCovertion(cpu["L3CacheSize"], 1)[1], "red"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"GPU: ", "red"});
        var gpus = Wmic("Win32_VideoController");
        count = 1;
        tab = string.Empty;
        foreach (var gpu in gpus) {
            if (gpus.Count > 1)
                order = count + ". ";
            else
                order = string.Empty;
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {gpu["Name"], "white"});
            Result.Add(new[] {"\n", string.Empty});
            Result.Add(new[] {Space + "   ", "red"});
            Result.Add(new[] {CapcityCovertion(gpu["AdapterRAM"])[0], "white"});
            Result.Add(new[] {" " + CapcityCovertion(gpu["AdapterRAM"])[1], "red"});
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {gpu["AdapterDACType"], "white"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"MEM: ", "red"});
        var mems = Wmic("Win32_PhysicalMemory");
        count = 1;
        tab = string.Empty;
        foreach (var mem in mems) {
            if (mems.Count > 1)
                order = count + ". ";
            else
                order = string.Empty;
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {mem["DeviceLocator"], "white"});
            Result.Add(new[] {" · ", "red"});
            var capacity = CapcityCovertion(mem["Capacity"]);
            Result.Add(new[] {capacity[0], "white"});
            Result.Add(new[] {" " + capacity[1], "red"});
            if (!string.IsNullOrEmpty(mem["ConfiguredClockSpeed"])) {
                Result.Add(new[] {" · ", "red"});
                Result.Add(new[] {mem["ConfiguredClockSpeed"], "white"});
                Result.Add(new[] {" MHZ", "red"});
            }

            if (!string.IsNullOrEmpty(mem["Manufacturer"])) {
                Result.Add(new[] {" · ", "red"});
                Result.Add(new[] {mem["Manufacturer"], "white"});
            }

            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"DSK: ", "red"});
        var dsks = Wmic("Win32_DiskDrive");
        count = 1;
        tab = string.Empty;
        var dskOn = (from dsk in dsks let size = CapcityCovertion(dsk["Size"]) where !string.IsNullOrEmpty(size[0]) select dsk).ToList();

        foreach (var dsk in dskOn) {
            if (dskOn.Count > 1)
                order = count + ". ";
            else
                order = string.Empty;
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            if (!string.IsNullOrEmpty(dsk["Caption"])) {
                Result.Add(new[] {dsk["Caption"], "white"});
                Result.Add(new[] {" · ", "red"});
            }

            var capacity = CapcityCovertion(dsk["Size"]);
            Result.Add(new[] {capacity[0], "white"});
            Result.Add(new[] {" " + capacity[1], "red"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"NIC: ", "red"});
        var nets = Wmic("Win32_NetworkAdapter");
        count = 1;
        tab = string.Empty;
        var netOn = nets.Where(net => Convert.ToBoolean(net["PhysicalAdapter"])).ToList();
        foreach (var net in netOn) {
            if (netOn.Count > 1)
                order = count + ". ";
            else
                order = string.Empty;
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {net["Name"], "white"});
            Result.Add(new[] {" · ", "red"});
            var speed = SpeedCovertion(Wmi(net["Name"])["LinkSpeed"]);
            Result.Add(new[] {speed[0], "white"});
            Result.Add(new[] {" " + speed[1], "red"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"\n", string.Empty});

        // ColorBlock();

        foreach (var line in Result)
            if (line[0] == "\n") {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                if (++_lineNum < Logo.Length)
                    Console.Write(Logo[_lineNum] + "    ");
                else
                    Console.Write("                                         ");
                Console.ResetColor();
            }
            else {
                Console.ForegroundColor = (ConsoleColor) Enum.Parse(typeof(ConsoleColor), UpperFirstLetter(line[1]));
                Console.Write(line[0]);
                Console.ResetColor();
            }

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Blue;
        while (_lineNum < Logo.Length) Console.WriteLine(Logo[_lineNum++]);
        Console.ResetColor();

        Console.ReadKey();
    }

    private static List<Dictionary<string, string>> Wmic(string strObject) {
        List<Dictionary<string, string>> results = new();
        var properties = Dict[strObject];
        Mc.Path = new ManagementPath(strObject);
        Mc.Options.UseAmendedQualifiers = false;
        var moc = Mc.GetInstances();
        foreach (var o in moc) {
            var mo = (ManagementObject) o;
            Dictionary<string, string> result = new();
            foreach (var property in properties) {
                result.Add(property, string.Empty);
                try {
                    var query = mo.Properties[property].Value.ToString();
                    if (!string.IsNullOrEmpty(query)) result[property] = query;
                }
                catch (Exception) {
                    // ignored
                }
            }

            results.Add(result);
        }

        return results;
    }

    private static Dictionary<string, string> Wmi(string name) {
        Dictionary<string, string> result = new() {
            {"LinkSpeed", string.Empty}
        };
        SelectQuery selectQuery = new("SELECT * FROM MSNdis_LinkSpeed WHERE active=true");
        ManagementScope scope = new("root\\wmi");
        ManagementObjectSearcher searcher = new(scope, selectQuery);
        foreach (var o in searcher.Get()) {
            var resultVal = (ManagementObject) o;
            if (resultVal["InstanceName"].ToString() == name)
                result["LinkSpeed"] = resultVal["NdisLinkSpeed"].ToString()!;
        }

        return result;
    }

    /*
    private static void ColorBlock() {
        var colors = (ConsoleColor[]) Enum.GetValues(typeof(ConsoleColor));
        foreach (var color in colors) Result.Add(new[] {"██", color.ToString()});
    }
    */

    private static void Uptime(string strBoot, string strLocal) {
        var boot = ManagementDateTimeConverter.ToDateTime(strBoot);
        var local = ManagementDateTimeConverter.ToDateTime(strLocal);
        var uptime = local - boot;
        if (uptime.Days > 0) {
            Result.Add(new[] {uptime.Days.ToString(), "white"});
            Result.Add(new[] {" D ", "red"});
        }

        if (uptime.Hours > 0) {
            Result.Add(new[] {uptime.Hours.ToString(), "white"});
            Result.Add(new[] {" H ", "red"});
        }

        if (uptime.Minutes > 0) {
            Result.Add(new[] {uptime.Minutes.ToString(), "white"});
            Result.Add(new[] {" M ", "red"});
        }

        if (uptime.Seconds <= 0) return;
        Result.Add(new[] {uptime.Seconds.ToString(), "white"});
        Result.Add(new[] {" S ", "red"});
    }

    private static List<string> CapcityCovertion(string s) {
        return CapcityCovertion(s, 0);
    }

    private static List<string> CapcityCovertion(string s, int i) {
        var unit = new[] {"B", "KB", "MB", "GB", "TB", "PB"};
        try {
            var d = double.Parse(s);
            if (string.IsNullOrEmpty(s)) return new List<string> {string.Empty, string.Empty};
            while (d >= 1024) {
                d /= 1024;
                i++;
            }

            return new List<string> {Math.Round(d, 2).ToString(CultureInfo.CurrentCulture), unit[i]};
        }
        catch (Exception) {
            return new List<string> {string.Empty, string.Empty};
        }
    }

    private static List<string> SpeedCovertion(string s) {
        var unit = new[] {"KB/s", "MB/s", "GB/s", "TB/s", "PB/s"};
        var unitIndex = 0;
        if (s == "9223372036854775807") return new List<string> {string.Empty, string.Empty};
        try {
            var d = double.Parse(s);
            while (d >= 1000) {
                d /= 1000;
                unitIndex++;
            }

            return new List<string> {Math.Round(d, 2).ToString(CultureInfo.CurrentCulture), unit[unitIndex]};
        }
        catch (Exception) {
            return new List<string> {string.Empty, string.Empty};
        }
    }

    private static string UpperFirstLetter(string s) {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return char.ToUpper(s[0]) + s[1..];
    }
}