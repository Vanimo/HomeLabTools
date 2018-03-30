using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerFanControlLib.IPMI
{
    class IpmiTool
    {
        public static String GetIpmiPath()
        {
            return @"Dependencies\bmc\ipmitool.exe";
        }

        // Builds the first part of the request's Command Line Arguments, the destination and credentials
        public static String GetCLAReference(String address, String user, String pwd)
        {
            return $"-I lanplus -H {address} -U {user} -P {pwd} -R 1";
        }

        // A second part of the CLA: Send a RAW command
        private static String GetCLARaw()
        {
            return " raw";
        }

        // Get RAW message to toggle the fan control
        public static Tuple<String, String> GetCLAFanControl(String address, String user, String pwd, Boolean manual)
        {
            return new Tuple<String, String>(GetIpmiPath(), $"{GetCLAReference(address, user, pwd)}{GetCLARaw()}{GetRawFanControl(manual)}");
        }

        // Get RAW message to toggle the fan control
        private static String GetRawFanControl(Boolean manual)
        {
            return $" 0x30 0x30 0x01 0x0{(manual ? "0" : "1")}";
        }

        public static String GetCLAFanSpeed(String reference, Int16 hexSpeed)
        {
            return $"{reference}{GetCLARaw()}{GetRawFanSpeed(hexSpeed)}";
        }

        // Get RAW message to set the fan speed
        private static String GetRawFanSpeed(Int16 hexSpeed)
        {
            return $" 0x30 0x30 0x02 0xff 0x{hexSpeed:X}";
        }

        public static Tuple<String, String> GetCLATemperature(String address, String user, String pwd)
        {
            return new Tuple<String, String>(GetIpmiPath(), $"{GetCLAReference(address, user, pwd)}{GetCLATemperature()}");
        }

        // A second part of the CLA: Get the temperatures
        private static String GetCLATemperature()
        {
            return " sdr type temperature";
        }

        //Temp             | 01h | ns  |  3.1 | Disabled
        //Temp             | 02h | ns  |  3.2 | Disabled
        //Temp             | 05h | ns  | 10.1 | Disabled
        //Temp             | 06h | ns  | 10.2 | Disabled
        //Ambient Temp     | 0Eh | ok  |  7.1 | 23 degrees C
        //Planar Temp      | 0Fh | ns  |  7.1 | Disabled
        //IOH THERMTRIP    | 5Dh | ns  |  7.1 | Disabled
        //CPU Temp Interf  | 76h | ns  |  7.1 | Disabled
        //Temp             | 0Ah | ns  |  8.1 | Disabled
        //Temp             | 0Bh | ns  |  8.1 | Disabled
        //Temp             | 0Ch | ns  |  8.1 | Disabled
        public static Regex GetTemperatureRegex()
        {
            var sb = new StringBuilder();

            // Beginning of line
            //sb.Append(@"^");

            sb.Append(@"(?<Name>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<SNum>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Status>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Unknown>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Reading>[^\|\r\n]*)");

            // End of line
            //sb.Append(@"$");

            return new Regex(sb.ToString());//, RegexOptions.Multiline);
        }
    }
}
