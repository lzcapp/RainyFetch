using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;

namespace RainyFetch {
    /// <summary>
    /// Thin, safe wrapper over the WMI classes used by RainyFetch.
    /// Every query is scoped to the properties the UI actually needs,
    /// results are fully disposed, and no row can ever produce an
    /// unhandled exception (a property that is missing or null simply
    /// yields an empty string, exactly like the original behaviour).
    /// </summary>
    internal static class HardwareInfo {
        /// <summary>Property sets queried per WMI class, in display order.</summary>
        private static readonly Dictionary<string, string[]> PropertySets = new() {
            { "Win32_ComputerSystem", ["UserName", "Name", "Manufacturer", "SystemFamily"] },
            { "Win32_OperatingSystem", ["Caption", "Version", "OSArchitecture", "LastBootUpTime", "LocalDateTime", "RegisteredUser"] },
            { "Win32_Processor", ["Name", "Caption", "CurrentClockSpeed", "MaxClockSpeed", "NumberOfCores", "NumberOfEnabledCore", "NumberOfLogicalProcessors", "ThreadCount", "L2CacheSize", "L3CacheSize", "Architecture"] },
            { "Win32_VideoController", ["Name", "AdapterRAM", "AdapterDACType"] },
            { "Win32_BaseBoard", ["Manufacturer", "Product", "SerialNumber", "Version"] },
            { "Win32_BIOS", ["Manufacturer", "Caption"] },
            { "Win32_PhysicalMemory", ["Capacity", "ConfiguredClockSpeed", "Manufacturer", "DeviceLocator"] },
            { "Win32_DiskDrive", ["Size", "Caption"] },
            { "Win32_NetworkAdapter", ["PhysicalAdapter", "Name", "Speed"] }
        };

        private static readonly string[] CapacityUnits = ["B", "KB", "MB", "GB", "TB", "PB"];
        private static readonly string[] SpeedUnits = ["bps", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps"];

        /// <summary>
        /// Queries a WMI class and returns one row per instance. Every requested
        /// property is guaranteed to be present in the row, with an empty string
        /// (or "Unknown" via <see cref="FirstRow"/>) when WMI could not provide it.
        /// </summary>
        internal static List<Dictionary<string, string>> Query(string className) {
            var rows = new List<Dictionary<string, string>>();
            if (!PropertySets.TryGetValue(className, out var properties))
                return rows;

            // Ask for only the columns we render; fall back to "SELECT *" on
            // systems where one of the newer columns is not recognised.
            try {
                var select = string.Join(",", properties);
                rows = Fetch($"SELECT {select} FROM {className}", properties);
                if (rows.Count == 0)
                    rows = Fetch($"SELECT * FROM {className}", properties);
            } catch (ManagementException) {
                try {
                    rows = Fetch($"SELECT * FROM {className}", properties);
                } catch (ManagementException) {
                    rows = [];
                }
            }

            return rows;
        }

        /// <summary>
        /// First row of a query, or a single row whose properties all read
        /// "Unknown" when the WMI class returned nothing (avoids index crashes
        /// on machines/virtual machines that hide a class).
        /// </summary>
        internal static Dictionary<string, string> FirstRow(string className) {
            var rows = Query(className);
            if (rows.Count > 0)
                return rows[0];

            var fallback = new Dictionary<string, string>(StringComparer.Ordinal);
            if (PropertySets.TryGetValue(className, out var properties))
                foreach (var property in properties)
                    fallback[property] = "Unknown";
            return fallback;
        }

        private static List<Dictionary<string, string>> Fetch(string wql, string[] properties) {
            var rows = new List<Dictionary<string, string>>();
            using var searcher = new ManagementObjectSearcher(wql);
            using (var instances = searcher.Get())
            {
                foreach (ManagementBaseObject o in instances)
                {
                    using var mo = (ManagementObject)o;
                    var row = new Dictionary<string, string>(properties.Length, StringComparer.Ordinal);
                    foreach (var property in properties)
                    {
                        row[property] = string.Empty;
                        try {
                            var value = mo.Properties[property]?.Value;
                            if (value is null)
                                continue;

                            var text = value.ToString()?.Trim();
                            if (string.IsNullOrEmpty(text))
                                continue;

                            row[property] = text.Replace("To be filled by O.E.M.", "");
                        } catch (Exception) {
                            // Property missing or unreadable on this system — keep it empty.
                        }
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }

        /// <summary>
        /// Formats a byte/cache size (WMI values are numeric strings) with a
        /// human readable unit. <paramref name="unitIndex"/> shifts the unit
        /// scale, e.g. L2/L3 cache sizes are reported in KB, not bytes, so the
        /// caller passes 1 to start the scale at KB.
        /// </summary>
        internal static (string Value, string Unit) ConvertCapacity(string raw, int unitIndex = 0) {
            if (string.IsNullOrEmpty(raw))
                return (string.Empty, string.Empty);
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return (string.Empty, string.Empty);

            var i = Math.Clamp(unitIndex, 0, CapacityUnits.Length - 1);
            while (value >= 1024 && i < CapacityUnits.Length - 1) {
                value /= 1024;
                i++;
            }

            return (Math.Round(value, 2).ToString(CultureInfo.CurrentCulture), CapacityUnits[i]);
        }

        /// <summary>
        /// Formats a link speed (bps) with a human readable unit. WMI reports
        /// "9223372036854775807" (long.MaxValue) when a NIC has no link, which
        /// is treated as "no value" like the original tool did.
        /// </summary>
        internal static (string Value, string Unit) ConvertSpeed(string raw) {
            if (string.IsNullOrEmpty(raw) || raw == "9223372036854775807")
                return (string.Empty, string.Empty);
            if (!decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return (string.Empty, string.Empty);

            var i = 0;
            while (value >= 1000 && i < SpeedUnits.Length - 1) {
                value /= 1000;
                i++;
            }

            return (Math.Round(value, 2).ToString(CultureInfo.CurrentCulture), SpeedUnits[i]);
        }
    }
}
