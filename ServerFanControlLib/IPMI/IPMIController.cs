using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ServerFanControlLib.Loggers;

namespace ServerFanControlLib.IPMI
{
    internal class IPMIController
    {
        private readonly IpmiServer m_server;

        public IPMIController(IpmiServer server)
        {
            m_server = server;
        }
        
        public String GetTemperatures(Boolean preFilter)
        {
            var sbResultTemps = new StringBuilder();
            using (var process = new Process())
            {
                process.StartInfo = m_server.GetTemperaturePSI();
                process.Start();
                
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (line == null)
                    {
                        continue;
                    }

                    if (preFilter)
                    {
                        if (!line.Contains("Ambient") && !line.Contains("Planar"))
                        {
                            continue;
                        }
                    }

                    sbResultTemps.AppendLine(line);
                }
            }

            return sbResultTemps.ToString();
        }

        public void SetFanByServerTemperature(Int32 temperature, bool includeManualControl)
        {
            if (includeManualControl)
            {
                Process.Start(m_server.GetManualControlPSI());
                EventLogger.Instance.Log("Set manual fan control for server " + m_server.m_address);
            }

            Process.Start(m_server.GetFixedSpeedPSI(temperature));
            EventLogger.Instance.Log("Set fan speed for server " + m_server.m_address + " with temperature " + temperature + " degC to " + m_server.GetSpeedForTemperature(temperature) + "dec");
        }

        public void SetAutomaticFanControl()
        {
            Process.Start(m_server.GetAutomaticControlPSI());
            EventLogger.Instance.Log("Set automatic fan control for server " + m_server.m_address);
        }

        public ServerTemps ParseServerTemps(String temperatures)
        {
            var tempsResults = m_server.TemperatureRegex.Matches(temperatures);

            var st = new ServerTemps();

            foreach (Match match in tempsResults)
            {
                if (match.Groups["Name"].Value.Contains("Ambient"))
                {
                    st.SetAmbient(match.Groups["Reading"].Value);
                }
                else if (match.Groups["Name"].Value.Contains("Planar"))
                {
                    st.SetMainboard(match.Groups["Reading"].Value);
                }
            }

            if (st.IsValid())
            {
                m_server.LogTemperatures(st);
            }

            return st;
        }
    }
}
