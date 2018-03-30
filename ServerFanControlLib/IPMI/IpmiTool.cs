﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
