using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerFanControlLib.IPMI
{
    class IpmiUtil
    {
        public static String GetIpmiPath()
        {
            return @"Dependencies\ipmiutil\ipmiutil.exe";
        }

        // Builds the first part of the request's Command Line Arguments, the destination and credentials
        public static String GetCLAReference(String address, String user, String pwd)
        {
            var sb = new StringBuilder();
            sb.Append(" -J ").Append("2");
            sb.Append(" -N ").Append(address);
            sb.Append(" -U ").Append(user);
            sb.Append(" -P ").Append(pwd);

            return sb.ToString();
        }

        public static Tuple<String, String> GetCLATemperature(String address, String user, String pwd)
        {
            return new Tuple<String, String>(GetIpmiPath(), $"sensor{GetCLAReference(address, user, pwd)}{GetCLATemperature()}");
        }

        // A second part of the CLA: Get the temperatures
        private static String GetCLATemperature()
        {
            return " -c -g \"Temperature\"";
        }

        public static Regex GetTemperatureRegex()
        {
            var sb = new StringBuilder();

            // Beginning of line
            //sb.Append(@"^");

            sb.Append(@"(?<ID>[0-9a-fA-F]{4})\s*").Append(@"\|");
            sb.Append(@"(?<SDRType>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Type>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<SNum>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Name>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Status>[^\|\r\n]*)").Append(@"\|");
            sb.Append(@"(?<Reading>[^\|\r\n]*)");

            // End of line
            //sb.Append(@"$");

            return new Regex(sb.ToString());//, RegexOptions.Multiline);
        }
    }
}
