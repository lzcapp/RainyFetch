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

    private static readonly ManagementClass Mc = new();
    private static int _lineNum;

    private static readonly Dictionary<string, string[]> Dict = new() {
        {"Win32_ComputerSystem", new[] {"UserName", "Name", "Manufacturer", "SystemFamily"}},
        {"Win32_OperatingSystem", new[] {"Caption", "Version", "OSArchitecture", "LastBootUpTime", "LocalDateTime"}},
        {"Win32_BaseBoard", new[] {"Manufacturer", "Product", "SerialNumber", "Version"}},
        {"Win32_PhysicalMemory", new[] {"Capacity", "ConfiguredClockSpeed", "Manufacturer"}},
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
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {dictBb[0]["Product"], "white"});
        }

        if (dictBb[0]["Version"] != "None") {
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {dictBb[0]["Version"], "white"});
        }

        Result.Add(new[] {"\n", string.Empty});
        if (dictBb[0]["SerialNumber"] != "None") {
            Result.Add(new[] {Space + dictBb[0]["SerialNumber"], "white"});
            Result.Add(new[] {"\n", string.Empty});
        }

        var dictOs = Wmic("Win32_OperatingSystem");
        Result.Add(new[] {"O S: ", "red"});
        Result.Add(new[] {dictOs[0]["Caption"], "white"});
        Result.Add(new[] {"\n", string.Empty});
        Result.Add(new[] {Space + dictOs[0]["Version"], "white"});
        Result.Add(new[] {" · ", "red"});
        Result.Add(new[] {dictOs[0]["OSArchitecture"], "white"});
        Result.Add(new[] {"\n", string.Empty});

        Result.Add(new[] {"U P: ", "red"});
        Uptime(dictOs[0]["LastBootUpTime"], dictOs[0]["LocalDateTime"]);
        Result.Add(new[] {"\n", string.Empty});

        Result.Add(new[] {"CPU: ", "red"});
        var cpus = Wmic("Win32_Processor", "Name");
        var count = 1;
        var tab = string.Empty;
        foreach (var cpu in cpus) {
            if (cpus.Count > 1)
                order = count + ". ";
            else
                order = "";
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {cpu, "white"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"GPU: ", "red"});
        var gpus = Wmic("Win32_VideoController", "Name");
        count = 1;
        tab = string.Empty;
        foreach (var gpu in gpus) {
            if (gpus.Count > 1)
                order = count + ". ";
            else
                order = "";
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {gpu, "white"});
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
                order = "";
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {CapcityCovertion(mem["Capacity"]), "white"});
            if (!string.IsNullOrEmpty(mem["ConfiguredClockSpeed"])) {
                Result.Add(new[] {" · ", "red"});
                Result.Add(new[] {mem["ConfiguredClockSpeed"] + " MHZ", "white"});
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
        var dskOn = (from dsk in dsks let size = CapcityCovertion(dsk["Size"]) where !string.IsNullOrEmpty(size) select dsk).ToList();

        foreach (var dsk in dskOn) {
            if (dskOn.Count > 1)
                order = count + ". ";
            else
                order = "";
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {CapcityCovertion(dsk["Size"]), "white"});
            if (!string.IsNullOrEmpty(dsk["Caption"])) {
                Result.Add(new[] {" · ", "red"});
                Result.Add(new[] {dsk["Caption"], "white"});
            }

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
                order = "";
            if (count > 1) tab = Space;
            Result.Add(new[] {tab + order, "red"});
            Result.Add(new[] {net["Name"], "white"});
            Result.Add(new[] {" · ", "red"});
            Result.Add(new[] {SpeedCovertion(Wmi(net["Name"])["LinkSpeed"]), "white"});
            Result.Add(new[] {"\n", string.Empty});
            count++;
        }

        Result.Add(new[] {"\n", string.Empty});

        ColorBlock();

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
                result.Add(property, "");
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

    private static List<string> Wmic(string strClass, string strObject) {
        Mc.Path = new ManagementPath(strClass);
        Mc.Options.UseAmendedQualifiers = true;
        var moc = Mc.GetInstances();
        List<string> result = new();
        foreach (var o in moc) {
            var mo = (ManagementObject) o;
            try {
                var query = mo.Properties[strObject].Value.ToString();
                if (!string.IsNullOrEmpty(query)) result.Add(query);
            }
            catch (Exception) {
                // ignored
            }
        }

        return result;
    }

    private static Dictionary<string, string> Wmi(string name) {
        Dictionary<string, string> result = new() {
            {"LinkSpeed", ""}
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

    private static void ColorBlock() {
        var colors = (ConsoleColor[]) Enum.GetValues(typeof(ConsoleColor));
        foreach (var color in colors) Result.Add(new[] {"██", color.ToString()});
    }

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

    private static string CapcityCovertion(string s) {
        var unit = new[] {"B", "KB", "MB", "GB", "TB", "PB"};
        var unitIndex = 0;
        try {
            var d = double.Parse(s);
            if (string.IsNullOrEmpty(s)) return string.Empty;
            while (d >= 1024) {
                d /= 1024;
                unitIndex++;
            }

            return Math.Round(d, 2).ToString(CultureInfo.CurrentCulture) + " " + unit[unitIndex];
        }
        catch (Exception) {
            return string.Empty;
        }
    }

    private static string SpeedCovertion(string s) {
        var unit = new[] {"KB/s", "MB/s", "GB/s", "TB/s", "PB/s"};
        var unitIndex = 0;
        if (s == "9223372036854775807") return "N/A";
        try {
            var d = double.Parse(s);
            while (d >= 1000) {
                d /= 1000;
                unitIndex++;
            }

            return Math.Round(d, 2).ToString(CultureInfo.CurrentCulture) + " " + unit[unitIndex];
        }
        catch (Exception) {
            return string.Empty;
        }
    }

    private static string UpperFirstLetter(string s) {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return char.ToUpper(s[0]) + s[1..];
    }
}