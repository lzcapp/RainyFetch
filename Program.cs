using System.Management;

namespace RainyFetch {
    class Program {
        private static readonly string[] logo = {
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
            { "Win32_ComputerSystem", new string[] { "UserName", "Name" } },
            { "Win32_OperatingSystem", new string[] { "Caption", "Version", "OSArchitecture" } },
            { "Win32_PhysicalMemory", new string[] { "Capacity" } }
        };
        private static readonly List<string[]> result = new();

        static void Main() {
            var count = 0;
            var tab = "";

            result.Add(new[] { "\n", "" });
            var dictCS = WMIC("Win32_ComputerSystem");
            string strUser = dictCS["UserName"][(dictCS["UserName"].IndexOf("\\") + 1)..].Trim();
            string strSystem = dictCS["Name"];

            result.Add(new[] { strUser, "red" });
            result.Add(new[] { "@", "white" });
            result.Add(new[] { strSystem, "red" });
            result.Add(new[] { "\n", "" });

            var hr = "";
            for (int len = 0; len < strUser.Length + strSystem.Length + 1; len++) {
                hr += "-";
            }
            result.Add(new[] { hr, "white" });
            result.Add(new[] { "\n", "" });

            var dictOS = WMIC("Win32_OperatingSystem");
            result.Add(new[] { "O S: ", "red" });
            result.Add(new[] { dictOS["Caption"], "white" });
            result.Add(new[] { "\n", "" });
            result.Add(new[] { "     " + dictOS["Version"] + " · " + dictOS["OSArchitecture"], "white" });
            result.Add(new[] { "\n", "" });

            result.Add(new[] { "CPU: ", "red" });
            var cpus = WMIC("Win32_Processor", "Name");
            count = 0;
            tab = "";
            foreach (var cpu in cpus) {
                if (count > 0) {
                    tab = "     ";
                }
                result.Add(new[] { tab + cpu, "white" });
                result.Add(new[] { "\n", "" });
                count++;
            }

            result.Add(new[] { "GPU: ", "red" });
            var gpus = WMIC("Win32_VideoController", "Name");
            count = 0;
            tab = "";
            foreach (var gpu in gpus) {
                if (count > 0) {
                    tab = "     ";
                }
                result.Add(new[] { tab + gpu, "white" });
                result.Add(new[] { "\n", "" });
                count++;
            }

            var dictMEM = WMIC("Win32_PhysicalMemory");
            dictMEM["Capacity"] = (double.Parse(dictMEM["Capacity"]) / 1024 / 1024 / 1024).ToString() + "GB";
            result.Add(new[] { "MEM: ", "red" });
            result.Add(new[] { dictMEM["Capacity"], "white" });
            result.Add(new[] { "\n", "" });

            result.Add(new[] { "\n", "" });
            ColorBlock();

            foreach (var line in result) {
                if (line[0] == "\n") {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(logo[intLineNum++] + "   ");
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), UpperFirstLetter(line[1]));
                    Console.Write(line[0]);
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            while (intLineNum < logo.Length) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(logo[intLineNum++] + "   ");
                Console.ResetColor();
            }

            Console.Write("  Press any key to exit...");
            Console.ReadKey();
        }

        private static Dictionary<string, string> WMIC(string strObject) {
            string[] properties = dict[strObject];
            Dictionary<string, string> wmicDict = new();
            mc.Path = new ManagementPath(strObject);
            mc.Options.UseAmendedQualifiers = false;
            ManagementObjectCollection moc = mc.GetInstances();

            for (int i = 0; i < properties.Length; i++) {
                if (properties.Contains("Capacity")) {
                    double intResult = 0;
                    foreach (ManagementObject mo in moc) {
                        var result = mo.Properties[properties[i]].Value.ToString();
                        if (!string.IsNullOrEmpty(result)) {
                            intResult += double.Parse(result);
                        }
                    }
                    wmicDict.Add(properties[i], intResult.ToString());
                } else {
                    var strResult = "";
                    foreach (ManagementObject mo in moc) {
                        strResult += mo.Properties[properties[i]].Value.ToString();
                    }
                    wmicDict.Add(properties[i], strResult);
                }
            }

            return wmicDict;
        }

        private static List<string> WMIC(string strClass, string strObject) {
            mc.Path = new ManagementPath(strClass);
            mc.Options.UseAmendedQualifiers = true;
            ManagementObjectCollection moc = mc.GetInstances();
            List<string> result = new();
            if (moc.Count >= 1) {
                foreach (ManagementObject mo in moc) {
                    var query = mo.Properties[strObject].Value.ToString();
                    if (!string.IsNullOrEmpty(query)) {
                        result.Add(query);
                    }
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

        public static string UpperFirstLetter(string s) {
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s[1..];
        }
    }
}