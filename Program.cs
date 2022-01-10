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
            { "Win32_OperatingSystem", new string[] { "Caption", "Version", "OSArchitecture" } }
        };

        static void Main() {
            Console.WriteLine();
            PrintLogo();

            var dictCS = WMIC("Win32_ComputerSystem");
            string strUser = dictCS["UserName"][(dictCS["UserName"].IndexOf("\\") + 1)..].Trim();
            string strSystem = dictCS["Name"];
            Write(strUser, ConsoleColor.Red);
            Write("@");
            Write(strSystem, ConsoleColor.Red);
            Console.WriteLine();

            PrintLogo();
            for (int len = 0; len < strUser.Length + strSystem.Length + 1; len++) {
                Console.Write("-");
            }
            Console.WriteLine();

            PrintLogo();
            var dictOS = WMIC("Win32_OperatingSystem");
            Query("O S", dictOS, "Caption");

            PrintLogo();
            Query(dictOS, "Version");

            PrintLogo();
            Query(dictOS, "OSArchitecture");

            PrintLogo();
            Query("CPU", "Win32_Processor", "Name");

            PrintLogo();
            Query("GPU", "Win32_VideoController", "Name");

            PrintLogo();
            Query("MEM", "Win32_PhysicalMemory", "Capacity");

            PrintLogo();
            Console.WriteLine();

            PrintLogo();
            ColorBlock();
            Console.WriteLine();

            int intTemp = logo.Length - intLineNum;
            for (int intLine = 0; intLine < intTemp; intLine++) {
                PrintLogo();
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.Write("  Press any key to exit...");
            Console.ReadKey();
        }

        private static Dictionary<string, string> WMIC(string strObject) {
            string[] properties = dict[strObject];
            Dictionary<string, string> wmicDict = new();
            mc.Path = new ManagementPath(strObject);
            mc.Options.UseAmendedQualifiers = true;
            ManagementObjectCollection moc = mc.GetInstances();

            for (int i = 0; i < properties.Length; i++) {
                var strResult = "";
                foreach (ManagementObject mo in moc) {
                    strResult += mo.Properties[properties[i]].Value.ToString();
                }
                wmicDict.Add(properties[i], strResult);
            }
            
            return wmicDict;
        }


        private static void Query(string strClass, string strObject) {
            Query("", strClass, strObject);
        }

        private static void Query(string strName, string strClass, string strObject) {
            string strResult = WMIC(strClass, strObject);
            Console.ForegroundColor = ConsoleColor.Red;
            if (string.IsNullOrEmpty(strName)) {
                Console.Write("     ");
            } else {
                Console.Write(strName + ": ");
            }
            Console.ResetColor();
            Console.WriteLine(strResult);
        }

        private static void Query(Dictionary<string, string> dictClass, string strObject) {
            Query("", dictClass, strObject);
        }

        private static void Query(string strName, Dictionary<string, string> dictClass, string strObject) {
            Console.ForegroundColor = ConsoleColor.Red;
            if (string.IsNullOrEmpty(strName)) {
                Console.Write("     ");
            } else {
                Console.Write(strName + ": ");
            }
            Console.ResetColor();
            Console.WriteLine(dictClass[strObject]);
        }

        private static string WMIC(string strClass, string strObject) {
            mc.Path = new ManagementPath(strClass);
            mc.Options.UseAmendedQualifiers = true;
            ManagementObjectCollection moc = mc.GetInstances();
            var strResult = "";
            int intCount = 1;
            if (moc.Count > 1) {
                foreach (ManagementObject mo in moc) {
                    if (intCount == 1) {
                        strResult += intCount + ". " + mo.Properties[strObject].Value.ToString();
                    } else {
                        strResult += "\n" + logo[intLineNum] + "        " + intCount + ". " + mo.Properties[strObject].Value.ToString();
                        intLineNum++;
                    }
                    intCount++;
                }
            } else {
                foreach (ManagementObject mo in moc) {
                    strResult += mo.Properties[strObject].Value.ToString();
                }
            }
            return strResult;
        }

        private static void PrintLogo() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(logo[intLineNum] + "   ");
            Console.ResetColor();
            intLineNum++;
        }

        private static void Write(string str) {
            Console.ResetColor();
            Console.Write(str);
            Console.ResetColor();
        }

        private static void Write(string str, ConsoleColor color) {
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ResetColor();
        }

        private static void ColorBlock() {
            ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            foreach (var color in colors) {
                Console.ForegroundColor = color;
                Console.Write("██");
                Console.ResetColor();
            }
        }
    }
}