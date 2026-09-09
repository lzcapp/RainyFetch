using System;
using System.Collections.Generic;
using System.Management;

namespace RainyFetch {
    internal static class Program {
        private const string Space = "     ";

        // The logo is drawn row-by-row as a left prefix for each content line;
        // every row must stay 37 chars wide so the column never shifts.
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

        // Win32_Processor.Architecture enum values -> readable name.
        private static readonly string[] ArchitectureNames = [
            "x86", "MIPS", "Alpha", "PowerPC", "", "ARM", "ia64",
            // Itanium-based systems
            "", "", "x64", "", "", "ARM64"
        ];

        // Width used when the logo rows run out: logo is 37 wide + 4 padding spaces.
        private const string LogoPad = "                                         ";

        private static readonly List<(string Text, ConsoleColor Color)> Result = [];
        private static int _lineNum;

        private static void Main() {
            Result.Add(("\n", ConsoleColor.Black));

            // Header — "<user>@<host> · <model>" then an underline of dashes.
            var dictCs = HardwareInfo.FirstRow("Win32_ComputerSystem");
            var strUser = ExtractUser(dictCs["UserName"]);
            var strSystem = dictCs["Name"];
            var strName = dictCs["Manufacturer"] + " " + dictCs["SystemFamily"];

            Add(strUser, ConsoleColor.Red);
            Add("@", ConsoleColor.White);
            Add(strSystem, ConsoleColor.Red);
            Add(" · ", ConsoleColor.White);
            Add(strName, ConsoleColor.Red);
            NewLine();

            var hr = new string('-', strUser.Length + strSystem.Length + strName.Length + 4);
            Add(hr, ConsoleColor.White);
            NewLine();

            // Motherboard & BIOS.
            var dictBb = HardwareInfo.FirstRow("Win32_BaseBoard");
            var dictBi = HardwareInfo.FirstRow("Win32_BIOS");
            Add("M B: ", ConsoleColor.Red);
            Add(dictBb["Manufacturer"], ConsoleColor.White);
            if (dictBb["Product"] != "None") {
                Add(" · ", ConsoleColor.Red);
                Add(dictBb["Product"], ConsoleColor.White);
            }
            NewLine();

            if (dictBb["SerialNumber"] != "None") {
                Add(Space + "SN: ", ConsoleColor.Red);
                Add(dictBb["SerialNumber"], ConsoleColor.White);
                NewLine();
            }

            Add(Space + "BIOS: ", ConsoleColor.Red);
            Add(dictBi["Manufacturer"], ConsoleColor.White);
            Add(" · ", ConsoleColor.Red);
            Add(dictBi["Caption"], ConsoleColor.White);
            NewLine();

            // Operating system + registered owner.
            var dictOs = HardwareInfo.FirstRow("Win32_OperatingSystem");
            Add("O S: ", ConsoleColor.Red);
            Add(dictOs["Caption"], ConsoleColor.White);
            Add(" · ", ConsoleColor.Red);
            Add(dictOs["Version"], ConsoleColor.White);
            NewLine();

            Add(Space + "Registered to ", ConsoleColor.Red);
            Add(dictOs["RegisteredUser"], ConsoleColor.White);
            NewLine();

            Add("U P: ", ConsoleColor.Red);
            Uptime(dictOs["LastBootUpTime"], dictOs["LocalDateTime"]);
            NewLine();

            CpuSection();
            GpuSection();
            MemorySection();
            DiskSection();
            NicSection();

            Result.Add(("\n", ConsoleColor.Black));

            // Render buffered lines; a "\n" marker ends the current line and
            // draws the next logo row (or padding) at the left margin.
            foreach (var (text, color) in Result)
                if (text == "\n") {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    if (++_lineNum < Logo.Length)
                        Console.Write(Logo[_lineNum] + "    ");
                    else
                        Console.Write(LogoPad);
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = color;
                    Console.Write(text);
                    Console.ResetColor();
                }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Blue;
            while (_lineNum < Logo.Length)
                Console.WriteLine(Logo[_lineNum++]);
            Console.ResetColor();

            // Keep the window open when double-clicked, but do not block when
            // stdout is redirected (piping to a file or another tool).
            if (!Console.IsOutputRedirected)
                Console.ReadKey();
        }

        private static void Add(string text, ConsoleColor color) {
            if (text.Length > 0)
                Result.Add((text, color));
        }

        private static void NewLine() {
            Result.Add(("\n", ConsoleColor.Black));
        }

        /// <summary>Renders a numbered item list: "1. xxx", "     2. yyy", ...</summary>
        private static void WriteItems(string title, IReadOnlyList<Dictionary<string, string>> items, Action<Dictionary<string, string>, string> body) {
            Add(title, ConsoleColor.Red);
            var count = 1;
            foreach (var item in items) {
                var order = items.Count > 1 ? count + ". " : string.Empty;
                var tab = count > 1 ? Space : string.Empty;
                body(item, tab + order);
                count++;
            }
        }

        private static void CpuSection() {
            var cpus = HardwareInfo.Query("Win32_Processor");
            WriteItems("CPU: ", cpus, (cpu, prefix) => {
                Add(prefix, ConsoleColor.Red);
                Add(cpu["Name"], ConsoleColor.White);
                NewLine();

                Add(Space + cpu["Caption"], ConsoleColor.White);
                NewLine();

                Add(Space + cpu["CurrentClockSpeed"], ConsoleColor.White);
                Add(" MHz", ConsoleColor.Red);
                Add(" · ", ConsoleColor.Red);
                Add("Max ", ConsoleColor.Red);
                Add(cpu["MaxClockSpeed"], ConsoleColor.White);
                Add(" MHz", ConsoleColor.Red);

                // Pluralise correctly per row instead of leaking state between CPUs.
                var strCore = cpu["NumberOfEnabledCore"] == "1" ? "core" : "cores";
                if (cpu["NumberOfCores"] == cpu["NumberOfEnabledCore"]) {
                    Add(" · " + cpu["NumberOfCores"], ConsoleColor.White);
                    Add(" " + strCore + " ", ConsoleColor.Red);
                } else {
                    Add(Space + cpu["NumberOfCores"], ConsoleColor.White);
                    Add("/", ConsoleColor.Red);
                    Add(cpu["NumberOfEnabledCore"], ConsoleColor.White);
                    Add(" " + strCore + " ", ConsoleColor.Red);
                }

                var strThread = cpu["ThreadCount"] == "1" ? "thread" : "threads";
                if (cpu["NumberOfLogicalProcessors"] == cpu["ThreadCount"]) {
                    Add(cpu["ThreadCount"], ConsoleColor.White);
                    Add(" " + strThread + " ", ConsoleColor.Red);
                } else {
                    Add(cpu["NumberOfLogicalProcessors"], ConsoleColor.White);
                    Add("/", ConsoleColor.Red);
                    Add(cpu["ThreadCount"], ConsoleColor.White);
                    Add(" " + strThread + " ", ConsoleColor.Red);
                }
                NewLine();

                Add(Space + "Architecture: ", ConsoleColor.Red);
                Add(ArchName(cpu["Architecture"]), ConsoleColor.White);
                Add(" · L2: ", ConsoleColor.Red);
                AddCapacity(cpu["L2CacheSize"], 1);
                Add(" · L3: ", ConsoleColor.Red);
                AddCapacity(cpu["L3CacheSize"], 1);
                NewLine();
            });
        }

        private static void GpuSection() {
            var gpus = HardwareInfo.Query("Win32_VideoController");
            WriteItems("GPU: ", gpus, (gpu, prefix) => {
                Add(prefix, ConsoleColor.Red);
                Add(gpu["Name"], ConsoleColor.White);
                if (!string.IsNullOrWhiteSpace(gpu["AdapterRAM"])) {
                    Add(" · ", ConsoleColor.Red);
                    AddCapacity(gpu["AdapterRAM"], 0);
                }
                if (!string.IsNullOrWhiteSpace(gpu["AdapterDACType"])) {
                    Add(" · ", ConsoleColor.Red);
                    Add(gpu["AdapterDACType"], ConsoleColor.White);
                }
                NewLine();
            });
        }

        private static void MemorySection() {
            var mems = HardwareInfo.Query("Win32_PhysicalMemory");
            WriteItems("MEM: ", mems, (mem, prefix) => {
                Add(prefix, ConsoleColor.Red);
                Add(mem["DeviceLocator"], ConsoleColor.White);
                Add(" · ", ConsoleColor.Red);
                AddCapacity(mem["Capacity"], 0);
                if (!string.IsNullOrEmpty(mem["ConfiguredClockSpeed"])) {
                    Add(" · ", ConsoleColor.Red);
                    Add(mem["ConfiguredClockSpeed"], ConsoleColor.White);
                    Add(" MHZ", ConsoleColor.Red);
                }
                if (!string.IsNullOrEmpty(mem["Manufacturer"])) {
                    Add(" · ", ConsoleColor.Red);
                    Add(mem["Manufacturer"], ConsoleColor.White);
                }
                NewLine();
            });
        }

        private static void DiskSection() {
            var dsks = HardwareInfo.Query("Win32_DiskDrive");
            var dskOn = new List<Dictionary<string, string>>();
            foreach (var dsk in dsks)
                if (!string.IsNullOrEmpty(HardwareInfo.ConvertCapacity(dsk["Size"]).Value))
                    dskOn.Add(dsk);

            WriteItems("DSK: ", dskOn, (dsk, prefix) => {
                Add(prefix, ConsoleColor.Red);
                if (!string.IsNullOrEmpty(dsk["Caption"])) {
                    Add(dsk["Caption"], ConsoleColor.White);
                    Add(" · ", ConsoleColor.Red);
                }
                AddCapacity(dsk["Size"], 0);
                NewLine();
            });
        }

        private static void NicSection() {
            var nets = HardwareInfo.Query("Win32_NetworkAdapter");
            var netOn = new List<Dictionary<string, string>>();
            foreach (var net in nets)
                if (net["PhysicalAdapter"] == "True")
                    netOn.Add(net);

            WriteItems("NIC: ", netOn, (net, prefix) => {
                Add(prefix, ConsoleColor.Red);
                Add(net["Name"], ConsoleColor.White);
                var speed = HardwareInfo.ConvertSpeed(net["Speed"]);
                if (!string.IsNullOrEmpty(speed.Value)) {
                    Add(" · ", ConsoleColor.Red);
                    if (speed.Value != "0") {
                        Add(speed.Value, ConsoleColor.White);
                        Add(" " + speed.Unit, ConsoleColor.Red);
                    } else {
                        Add("No Connection", ConsoleColor.Red);
                    }
                }
                NewLine();
            });
        }

        /// <summary>Appends a converted capacity as two coloured segments: value + unit.</summary>
        private static void AddCapacity(string raw, int unitIndex) {
            var (value, unit) = HardwareInfo.ConvertCapacity(raw, unitIndex);
            if (string.IsNullOrEmpty(value))
                return;
            Add(value, ConsoleColor.White);
            Add(" " + unit, ConsoleColor.Red);
        }

        private static string ArchName(string raw) {
            if (!int.TryParse(raw, out var index) || index < 0 || index >= ArchitectureNames.Length)
                return string.Empty;
            return ArchitectureNames[index];
        }

        private static string ExtractUser(string userName) {
            var index = userName.IndexOf("\\", StringComparison.Ordinal) + 1;
            var user = index >= 0 ? userName[index..] : string.Empty;
            return string.IsNullOrWhiteSpace(user) ? "Unknown" : user.Trim();
        }

        private static void Uptime(string strBoot, string strLocal) {
            try {
                var boot = ManagementDateTimeConverter.ToDateTime(strBoot);
                var local = ManagementDateTimeConverter.ToDateTime(strLocal);
                var uptime = local - boot;

                if (uptime.Days > 0) {
                    Add(uptime.Days.ToString(), ConsoleColor.White);
                    Add(" D ", ConsoleColor.Red);
                }
                if (uptime.Hours > 0) {
                    Add(uptime.Hours.ToString(), ConsoleColor.White);
                    Add(" H ", ConsoleColor.Red);
                }
                if (uptime.Minutes > 0) {
                    Add(uptime.Minutes.ToString(), ConsoleColor.White);
                    Add(" M ", ConsoleColor.Red);
                }
                if (uptime.Seconds > 0) {
                    Add(uptime.Seconds.ToString(), ConsoleColor.White);
                    Add(" S ", ConsoleColor.Red);
                }
            } catch (FormatException) {
                // Boot/local time missing or unreadable — print nothing after "U P: ".
            }
        }
    }
}
