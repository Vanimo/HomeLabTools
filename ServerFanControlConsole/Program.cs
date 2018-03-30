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

            m_pingTimer = new Timer(m_config.Interval * 1000);
            m_pingTimer.Elapsed += PingTimer_Tick;
            m_pingTimer.Start();

            UpdateServiceFromConfig();

            PingTimer_Tick(null, EventArgs.Empty);
            
            var bKeepRunning = false;
            if (args != null && args.Length > 0)
            {
                if (args[0] == "-R")
                {
                    bKeepRunning = true;
                }
            }

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

            if (Math.Abs(newInterval - m_pingTimer.Interval) > 1)
            {
                m_pingTimer.Interval = newInterval;
            }

            Cluster.LoadServers(m_config.Servers);
        }
        
        private static void PingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Cluster.UpdateAllServers();
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Log(ex);
            }
        }
    }
}
