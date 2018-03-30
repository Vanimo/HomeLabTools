using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServerFanControlLib.IPMI;
using ServerFanControlLib.Loggers;
using ServerFanControlLib.XML;

namespace ServerFanControlLib.IPMI
{
    public class IpmiServer
    {
        public readonly String m_address;
        private readonly String m_user;
        private readonly String m_pwd;

        public readonly Tuple<String, String> CmdGetTemperature;
        public readonly Tuple<String, String> CmdSetAutomaticControl;
        public readonly Tuple<String, String> CmdSetManualControl;
        private readonly Dictionary<Int32, String> m_cmdDictionarySpeedCmdForTemp = new Dictionary<Int32, String>();
        private readonly Dictionary<Int32, Int16> m_cmdDictionarySpeedForTemp = new Dictionary<Int32, Int16>();

        private Int32 m_lowerBound;
        private Int32 m_upperBound;

        public readonly Regex TemperatureRegex;

        public IpmiServer(XmlServer serverConfig)
        {
            m_address = serverConfig.Address;
            m_user = serverConfig.Username;
            m_pwd = serverConfig.Password;

            //CmdGetTemperature = IpmiUtil.GetCLATemperature(m_address, m_user, m_pwd);
            CmdGetTemperature = IpmiTool.GetCLATemperature(m_address, m_user, m_pwd);
            CmdSetAutomaticControl = IpmiTool.GetCLAFanControl(m_address, m_user, m_pwd, false);
            CmdSetManualControl = IpmiTool.GetCLAFanControl(m_address, m_user, m_pwd, true);

            TemperatureRegex = IpmiTool.GetTemperatureRegex();

            InitializeDefaultSpeedControl();
        }
        
        /* dec  hex     RPM         C   F
         * 00	00	    1200		
         * 01	01	    1200		
         * 02	02	    1320		
         * 03	03	    1560	    20	68
         * 04	04	    1680	    21	69,8
         * 05	05	    1800	    22	71,6
         * 06	06	    1920	    23	73,4
         * 07	07	    2040	    24	75,2
         * 08	08	    2160		
         * 09	09	    2280		
         * 10	0A	    2400		
         * 11	0B	    2400(-2520)		
         * 12	0C	    2520	    25	77
         * 13	0D	    2640		
         * 14	0E	    2760		
         * 15	0F	    2880		
         * 16	10	    3000		
         * 17	11	    3120	    26	78,8
         * 18	12	    3240		
         * 19	13	    3240(-3360)		
         * 20	14	    3360		
         * 21	15	    3480		
         * 22	16	    3600	    27	80,6    // Lowest/Default when running auto
         * 23	17	    3720		
         * 24	18	    3840		
         * 25	19	    3960		
         * 26	1A	    3960(-4200)		
         * 27	1B	    4200		
         * 28	1C	    4320		
         * 29	1D	    4440		
         * 30	1E	    4560		
         * 31	1F	    4560
         */
        private void InitializeDefaultSpeedControl()
        {
            var reference = IpmiTool.GetCLAReference(m_address, m_user, m_pwd);
            m_lowerBound = 20;
            AddSpeedToDictionaries(reference, 20, 3);
            AddSpeedToDictionaries(reference, 21, 4);
            AddSpeedToDictionaries(reference, 22, 5);
            AddSpeedToDictionaries(reference, 23, 6);
            AddSpeedToDictionaries(reference, 24, 7);
            AddSpeedToDictionaries(reference, 25, 12);
            AddSpeedToDictionaries(reference, 26, 17);
            m_upperBound = 26;
        }

        private void AddSpeedToDictionaries(String ipmiReference, Int32 temp, Int16 speed)
        {
            m_cmdDictionarySpeedCmdForTemp.Add(temp, IpmiTool.GetCLAFanSpeed(ipmiReference, speed));
            m_cmdDictionarySpeedForTemp.Add(temp, speed);
        }

        public Boolean IsManualTemperature(Int32 degC)
        {
            return degC <= m_upperBound;
        }

        public String GetSpeedCommand(Int32 degC)
        {
            degC = DictionaryTemperature(degC);
            return m_cmdDictionarySpeedCmdForTemp[degC];
        }

        public ProcessStartInfo GetTemperaturePSI()
        {
            return GetPSI(CmdGetTemperature);
        }

        public ProcessStartInfo GetAutomaticControlPSI()
        {
            return GetPSI(CmdSetAutomaticControl);
        }

        public ProcessStartInfo GetManualControlPSI()
        {
            return GetPSI(CmdSetManualControl);
        }

        public ProcessStartInfo GetFixedSpeedPSI(Int32 degC)
        {
            return GetPSI(new Tuple<String, String>(IpmiTool.GetIpmiPath(), GetSpeedCommand(degC)));
        }

        private ProcessStartInfo GetPSI(Tuple<String, String> command)
        {
            DebugLogger.Instance.Log("IpmiServer.GetPSI", "CMD", command.Item2);
            return new ProcessStartInfo
            {
                FileName = command.Item1,
                Arguments = command.Item2,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }

        public void LogTemperatures(ServerTemps temperatures)
        {
            EventLogger.Instance.Log($"Received from {m_address} AmbientTemp={temperatures.PrintAmbient} PlanarTemp={temperatures.PrintBoard}");
        }

        public Int16 GetSpeedForTemperature(Int32 degC)
        {
            degC = DictionaryTemperature(degC);
            return m_cmdDictionarySpeedForTemp[degC];
        }

        private Int32 DictionaryTemperature(Int32 degC)
        {
            if (degC < m_lowerBound)
            {
                degC = m_lowerBound;
            }
            else if (degC > m_upperBound)
            {
                degC = m_upperBound;
            }

            return degC;
        }
    }
}
