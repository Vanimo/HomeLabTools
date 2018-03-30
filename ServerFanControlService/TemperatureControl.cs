using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ServerFanControlLib;
using ServerFanControlLib.IPMI;
using ServerFanControlLib.Loggers;
using ServerFanControlLib.XML;

namespace ServerFanControlService
{
    public partial class TemperatureControl : ServiceBase
    {
        private XmlConfig m_config;
        private readonly ClusterController m_cluster = new ClusterController();

        public TemperatureControl()
        {
            InitializeComponent();

            EventLogger.Instance.Log("HomeLab Fan Controller Service Constructed");
        }

        protected override void OnStart(String[] args)
        {
            try
            {
                m_config = XmlHelper.GetOrCreateConfig();
                UpdateServiceFromConfig();

                ConfigWatcher.Filter = XmlHelper.ConfigFileName;
                ConfigWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                ErrorLogger.Instance.Log(e);
            }

            PingTimer.Enabled = true;
            PingTimer.Start();
            PingTimer_Tick(null, EventArgs.Empty);

            EventLogger.Instance.Log("HomeLab Fan Controller Service Started");
        }

        protected override void OnStop()
        {
            PingTimer.Enabled = false;
            PingTimer.Stop();
            EventLogger.Instance.Log("HomeLab Fan Controller Service Stopped");
        }

        protected override void OnPause()
        {
            PingTimer.Enabled = false;
            EventLogger.Instance.Log("HomeLab Fan Controller Service Paused");
        }

        protected override void OnContinue()
        {
            PingTimer.Enabled = true;
            PingTimer_Tick(null, EventArgs.Empty);
            EventLogger.Instance.Log("HomeLab Fan Controller Service Continued");
        }

        private void ConfigWatcher_Changed(Object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                m_config = XmlHelper.GetOrCreateConfig();
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Log(ex);
            }
        }

        private void UpdateServiceFromConfig()
        {
            var newInterval = m_config.Interval * 1000;

            if (newInterval != PingTimer.Interval)
            {
                PingTimer.Interval = newInterval;
                EventLogger.Instance.Log("HomeLab Fan Controller Ping Interval Changed: " + PingTimer.Interval + "ms");
            }

            m_cluster.LoadServers(m_config.Servers);
        }

        private void PingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                m_cluster.UpdateAllServers();
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Log(ex);
            }
        }
    }
}
