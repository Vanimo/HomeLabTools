using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerFanControlLib.Loggers;
using ServerFanControlLib.XML;

namespace ServerFanControlLib
{
    public class ClusterController
    {
        private readonly List<ServerTemperatureController> m_serverTemperatureControllers = new List<ServerTemperatureController>();

        public void LoadServers(IEnumerable<XmlServer> servers)
        {
            m_serverTemperatureControllers.Clear();
            foreach (var server in servers)
            {
                m_serverTemperatureControllers.Add(new ServerTemperatureController(server));
            }
        }

        public async Task UpdateAllServers()
        {
            var tasks = new List<Task<ServerTemperatureController.UpdateResult>>();

            foreach (var server in m_serverTemperatureControllers)
            {
                tasks.Add(server.Update());
            }

            foreach (var result in await Task.WhenAll(tasks))
            {
                EventLogger.Instance.Log("Updated Server " + result.ServerDestination + " with status " + result.SimpleStatus + " and temperature=" + result.CurrentTemperature + "°C and fan speed=" + result.CurrentFanSpeed + "rpm");
            }
        }

        public void ChangeAllToAutomatic()
        {
            foreach (var server in m_serverTemperatureControllers)
            {
                server.SetToAutomatic();
            }
        }
    }
}
