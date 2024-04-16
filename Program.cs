using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;

namespace RainyFetch {
    internal static class Program {
        private const string Space = "     ";

        private static readonly string[] Logo = [
            "                                  ..,",
            "                      ....,,:;+ccllll",
            "        ...,,+:;  cllllllllllllllllll",
            "  ,cclllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "                                     ",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  llllllllllllll  lllllllllllllllllll",
            "  `'ccllllllllll  lllllllllllllllllll",
            @"        `' \\*::  :ccllllllllllllllll",
            "                         ````''*::cll",
            "                                   ``"
        ];

        private static readonly string[] Architecture = [
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
        ];

        private static readonly ManagementClass Mc = new();
        private static int _lineNum;

        private static readonly Dictionary<string, string[]> Dict = new() {
            {"Win32_ComputerSystem", ["UserName", "Name", "Manufacturer", "SystemFamily"]
            },
            {"Win32_OperatingSystem", ["Caption", "Version", "OSArchitecture", "LastBootUpTime", "LocalDateTime", "RegisteredUser"]
            },
            {"Win32_Processor", ["Name", "Caption", "CurrentClockSpeed", "MaxClockSpeed", "NumberOfCores", "NumberOfEnabledCore", "NumberOfLogicalProcessors", "ThreadCount", "L2CacheSize", "L3CacheSize", "Architecture"]
            },
            {"Win32_VideoController", ["Name", "AdapterRAM", "AdapterDACType"]
            },
            {"Win32_BaseBoard", ["Manufacturer", "Product", "SerialNumber", "Version"]
            },
            {"Win32_BIOS", ["Manufacturer", "Caption"]
            },
            {"Win32_PhysicalMemory", ["Capacity", "ConfiguredClockSpeed", "Manufacturer", "DeviceLocator"]
            },
            {"Win32_DiskDrive", ["Size", "Caption"]
            },
            {"Win32_NetworkAdapter", ["PhysicalAdapter", "Name", "Speed"]
            }
        };

        private static readonly List<string[]> Result = [];

        private static void Main() {
            string order;

            Result.Add(["\n", string.Empty]);
            var dictCs = Wmic("Win32_ComputerSystem");
            var strUser = dictCs[0]["UserName"][(dictCs[0]["UserName"].IndexOf("\\", StringComparison.Ordinal) + 1)..]
                .Trim();
            var strSystem = dictCs[0]["Name"];
            var strName = dictCs[0]["Manufacturer"] + " " + dictCs[0]["SystemFamily"];

            Result.Add([strUser, "red"]);
            Result.Add(["@", "white"]);
            Result.Add([strSystem, "red"]);
            Result.Add([" · ", "white"]);
            Result.Add([strName, "red"]);
            Result.Add(["\n", string.Empty]);

            var hr = string.Empty;
            for (var len = 0; len < strUser.Length + strSystem.Length + strName.Length + 4; len++) hr += "-";
            Result.Add([hr, "white"]);
            Result.Add(["\n", string.Empty]);

            var dictBb = Wmic("Win32_BaseBoard");
            var dictBi = Wmic("Win32_BIOS");
            Result.Add(["M B: ", "red"]);
            Result.Add([dictBb[0]["Manufacturer"], "white"]);
            if (dictBb[0]["Product"] != "None") {
                Result.Add([" · ", "red"]);
                Result.Add([dictBb[0]["Product"], "white"]);
            }

            /*if (dictBb[0]["Version"] != "None") {
                Result.Add(new[] {" · ", "red"});
                Result.Add(new[] {dictBb[0]["Version"], "white"});
            }*/

            Result.Add(["\n", string.Empty]);
            if (dictBb[0]["SerialNumber"] != "None") {
                Result.Add([Space + "SN: ", "red"]);
                Result.Add([dictBb[0]["SerialNumber"], "white"]);
                Result.Add(["\n", string.Empty]);
            }
        
            Result.Add([Space + "BIOS: ", "red"]);
            Result.Add([dictBi[0]["Manufacturer"], "white"]);
            Result.Add([" · ", "red"]);
            Result.Add([dictBi[0]["Caption"], "white"]);
            Result.Add(["\n", string.Empty]);

            var dictOs = Wmic("Win32_OperatingSystem");
            Result.Add(["O S: ", "red"]);
            Result.Add([dictOs[0]["Caption"], "white"]);
            Result.Add([" · ", "red"]);
            Result.Add([dictOs[0]["Version"], "white"]);
            Result.Add(["\n", string.Empty]);
            Result.Add([Space + "Registered to ", "red"]);
            Result.Add([dictOs[0]["RegisteredUser"], "white"]);
            Result.Add(["\n", string.Empty]);

            Result.Add(["U P: ", "red"]);
            Uptime(dictOs[0]["LastBootUpTime"], dictOs[0]["LocalDateTime"]);
            Result.Add(["\n", string.Empty]);

            Result.Add(["CPU: ", "red"]);
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
                Result.Add([tab + order, "red"]);
                Result.Add([cpu["Name"], "white"]);
                Result.Add(["\n", string.Empty]);
                Result.Add([Space + cpu["Caption"], "white"]);
                Result.Add(["\n", string.Empty]);
                Result.Add([Space + cpu["CurrentClockSpeed"], "white"]);
                Result.Add([" MHz", "red"]);
                Result.Add([" · ", "red"]);
                Result.Add(["Max ", "red"]);
                Result.Add([cpu["MaxClockSpeed"], "white"]);
                Result.Add([" MHz", "red"]);
                if (cpu["NumberOfEnabledCore"] == "1") strCore = "core";
                if (cpu["NumberOfCores"] == cpu["NumberOfEnabledCore"]) {
                    Result.Add([" · " + cpu["NumberOfCores"], "white"]);
                    Result.Add([" " + strCore + " ", "red"]);
                } else {
                    Result.Add([Space + cpu["NumberOfCores"], "white"]);
                    Result.Add(["/", "red"]);
                    Result.Add([cpu["NumberOfEnabledCore"], "white"]);
                    Result.Add([" " + strCore + " ", "red"]);
                }

                if (cpu["ThreadCount"] == "1") strThread = "thread";
                if (cpu["NumberOfLogicalProcessors"] == cpu["ThreadCount"]) {
                    Result.Add([cpu["ThreadCount"], "white"]);
                    Result.Add([" " + strThread + " ", "red"]);
                } else {
                    Result.Add([cpu["NumberOfLogicalProcessors"], "white"]);
                    Result.Add(["/", "red"]);
                    Result.Add([cpu["ThreadCount"], "white"]);
                    Result.Add([" " + strThread + " ", "red"]);
                }

                Result.Add(["\n", string.Empty]);
                Result.Add([Space + "Architecture: ", "red"]);
                Result.Add([Architecture[int.Parse(cpu["Architecture"])], "white"]);
                Result.Add([" · L2: ", "red"]);
                Result.Add([CapcityCovertion(cpu["L2CacheSize"])[0], "white"]);
                Result.Add([" " + CapcityCovertion(cpu["L2CacheSize"], 1)[1], "red"]);
                Result.Add([" · L3: ", "red"]);
                Result.Add([CapcityCovertion(cpu["L3CacheSize"])[0], "white"]);
                Result.Add([" " + CapcityCovertion(cpu["L3CacheSize"], 1)[1], "red"]);
                Result.Add(["\n", string.Empty]);
                count++;
            }

            Result.Add(["GPU: ", "red"]);
            var gpus = Wmic("Win32_VideoController");
            count = 1;
            tab = string.Empty;
            foreach (var gpu in gpus) {
                if (gpus.Count > 1)
                    order = count + ". ";
                else
                    order = string.Empty;
                if (count > 1) tab = Space;
                Result.Add([tab + order, "red"]);
                Result.Add([gpu["Name"], "white"]);
                if (!string.IsNullOrEmpty(gpu["AdapterRAM"].Trim())) {
                    Result.Add([" · ", "red"]);
                    Result.Add([CapcityCovertion(gpu["AdapterRAM"])[0], "white"]);
                    Result.Add([" " + CapcityCovertion(gpu["AdapterRAM"])[1], "red"]);
                }
                if (!string.IsNullOrEmpty(gpu["AdapterDACType"].Trim())) {
                    Result.Add([" · ", "red"]);
                    Result.Add([gpu["AdapterDACType"], "white"]);
                }
                Result.Add(["\n", string.Empty]);
                count++;
            }

            Result.Add(["MEM: ", "red"]);
            var mems = Wmic("Win32_PhysicalMemory");
            count = 1;
            tab = string.Empty;
            foreach (var mem in mems) {
                if (mems.Count > 1)
                    order = count + ". ";
                else
                    order = string.Empty;
                if (count > 1) tab = Space;
                Result.Add([tab + order, "red"]);
                Result.Add([mem["DeviceLocator"], "white"]);
                Result.Add([" · ", "red"]);
                var capacity = CapcityCovertion(mem["Capacity"]);
                Result.Add([capacity[0], "white"]);
                Result.Add([" " + capacity[1], "red"]);
                if (!string.IsNullOrEmpty(mem["ConfiguredClockSpeed"])) {
                    Result.Add([" · ", "red"]);
                    Result.Add([mem["ConfiguredClockSpeed"], "white"]);
                    Result.Add([" MHZ", "red"]);
                }

                if (!string.IsNullOrEmpty(mem["Manufacturer"])) {
                    Result.Add([" · ", "red"]);
                    Result.Add([mem["Manufacturer"], "white"]);
                }

                Result.Add(["\n", string.Empty]);
                count++;
            }

            Result.Add(["DSK: ", "red"]);
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
                Result.Add([tab + order, "red"]);
                if (!string.IsNullOrEmpty(dsk["Caption"])) {
                    Result.Add([dsk["Caption"], "white"]);
                    Result.Add([" · ", "red"]);
                }

                var capacity = CapcityCovertion(dsk["Size"]);
                Result.Add([capacity[0], "white"]);
                Result.Add([" " + capacity[1], "red"]);
                Result.Add(["\n", string.Empty]);
                count++;
            }

            Result.Add(["NIC: ", "red"]);
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
                Result.Add([tab + order, "red"]);
                Result.Add([net["Name"], "white"]);
                //var speed_wmi = Wmi(net["Name"])["LinkSpeed"];
                var speed_w32 = net["Speed"];
                var speed = SpeedCovertion(speed_w32);
                if (!string.IsNullOrEmpty(speed[0])) {
                    Result.Add([" · ", "red"]);
                    if (speed[0] != "0") {
                        Result.Add([speed[0], "white"]);
                        Result.Add([" " + speed[1], "red"]);
                    } else {
                        Result.Add(["No Connection", "red"]);
                    }
                }
                Result.Add(["\n", string.Empty]);
                count++;
            }

            Result.Add(["\n", string.Empty]);

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
                } else {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), UpperFirstLetter(line[1]));
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
            List<Dictionary<string, string>> results = [];
            var properties = Dict[strObject];
            Mc.Path = new ManagementPath(strObject);
            Mc.Options.UseAmendedQualifiers = false;
            ManagementObjectCollection moc = Mc.GetInstances();
            foreach (ManagementBaseObject o in moc) {
                var mo = (ManagementObject)o;
                Dictionary<string, string> result = new();
                foreach (var property in properties) {
                    result.Add(property, string.Empty);
                    try {
                        var query = mo.Properties[property].Value.ToString();
                        if (!string.IsNullOrEmpty(query)) result[property] = query.Replace("To be filled by O.E.M.", "");
                    } catch (Exception) {
                        // ignored
                    }
                }

                results.Add(result);
            }

            return results;
        }

        /*
        private static Dictionary<string, string> Wmi(string name) {
            Dictionary<string, string> result = new() {
                {"LinkSpeed", string.Empty}
            };
            SelectQuery selectQuery = new("SELECT * FROM MSNdis_LinkSpeed WHERE active=true");
            ManagementScope scope = new("root\\wmi");
            ManagementObjectSearcher searcher = new(scope, selectQuery);
            foreach (var o in searcher.Get()) {
                var resultVal = (ManagementObject)o;
                if (resultVal["InstanceName"].ToString() == name)
                    result["LinkSpeed"] = resultVal["NdisLinkSpeed"].ToString()!;
            }

            return result;
        }
        */

        /*
        private static void ColorBlock() {
            var colors = (ConsoleColor[]) Enum.GetValues(typeof(ConsoleColor));
            foreach (var color in colors) Result.Add(new[] {"██", color.ToString()});
        }
        */

        private static void Uptime(string strBoot, string strLocal) {
            var boot = ManagementDateTimeConverter.ToDateTime(strBoot);
            var local = ManagementDateTimeConverter.ToDateTime(strLocal);
            TimeSpan uptime = local - boot;
            if (uptime.Days > 0) {
                Result.Add([uptime.Days.ToString(), "white"]);
                Result.Add([" D ", "red"]);
            }

            if (uptime.Hours > 0) {
                Result.Add([uptime.Hours.ToString(), "white"]);
                Result.Add([" H ", "red"]);
            }

            if (uptime.Minutes > 0) {
                Result.Add([uptime.Minutes.ToString(), "white"]);
                Result.Add([" M ", "red"]);
            }

            if (uptime.Seconds <= 0) return;
            Result.Add([uptime.Seconds.ToString(), "white"]);
            Result.Add([" S ", "red"]);
        }

        private static List<string> CapcityCovertion(string s, int i = 0) {
            var unit = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
            try {
                var d = double.Parse(s);
                if (string.IsNullOrEmpty(s)) return [string.Empty, string.Empty];
                while (d >= 1024) {
                    d /= 1024;
                    i++;
                }

                return [Math.Round(d, 2).ToString(CultureInfo.CurrentCulture), unit[i]];
            } catch (Exception) {
                return [string.Empty, string.Empty];
            }
        }

        private static List<string> SpeedCovertion(string s) {
            var unit = new[] { "bps", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps" };
            var unitIndex = 0;
            if (s == "9223372036854775807") return [string.Empty, string.Empty];
            try {
                var d = decimal.Parse(s);
                while (d >= 1000) {
                    d /= 1000;
                    unitIndex++;
                }

                return [Math.Round(d, 2).ToString(CultureInfo.CurrentCulture), unit[unitIndex]];
            } catch (Exception) {
                return [string.Empty, string.Empty];
            }
        }

        private static string UpperFirstLetter(string s) {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return char.ToUpper(s[0]) + s[1..];
        }
    }
}