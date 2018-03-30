using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using ServerFanControlLib.Loggers;

namespace ServerFanControlConsole
{
    using ServerFanControlLib;
    using ServerFanControlLib.XML;
    using ServerFanControlLib.IPMI;

    class Program
    {
        private static Timer m_pingTimer;
        private static XmlConfig m_config;
        private static readonly ClusterController Cluster = new ClusterController();

        /// <summary>
        /// Updates fan speed configured
        /// </summary>
        /// <param name="args">-R to keep running indefinately, unless escape is pressed</param>
        static void Main(string[] args)
        {
            m_config = XmlHelper.GetOrCreateConfig();
            
            UpdateServiceFromConfig();
            
            var bKeepRunning = false;
            if (args != null && args.Length > 0)
            {
                // R for repeat
                if (args[0] == "-R")
                {
                    bKeepRunning = true;

                    m_pingTimer = new Timer(m_config.Interval * 1000);
                    m_pingTimer.Elapsed += PingTimer_Tick;
                    m_pingTimer.Start();
                }

                // A for automatic
                if (args[0] == "-A")
                {
                    Cluster.ChangeAllToAutomatic();
                    return;
                }
            }

            UpdateOnce();

            while (bKeepRunning)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    bKeepRunning = false;
                }
            }
        }

        private static void UpdateServiceFromConfig()
        {
            var newInterval = m_config.Interval * 1000.0;

            if (m_pingTimer != null && Math.Abs(newInterval - m_pingTimer.Interval) > 1)
            {
                m_pingTimer.Interval = newInterval;
            }

            Cluster.LoadServers(m_config.Servers);
        }

        private static void UpdateOnce()
        {
            Cluster.UpdateAllServers();
        }
        
        private static void PingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Cluster.UpdateAllServersAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Log(ex);
            }
        }
    }
}
