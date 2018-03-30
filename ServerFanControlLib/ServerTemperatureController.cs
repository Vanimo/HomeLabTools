using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerFanControlLib.IPMI;
using ServerFanControlLib.Loggers;
using ServerFanControlLib.XML;

namespace ServerFanControlLib
{
    public class ServerTemperatureController
    {
        private readonly IpmiServer m_ipmiServer;

        private Boolean m_wasAutomatic;
        private Boolean m_wasPreviousSuccess;
        
        private Int16 m_previousIpmiSpeed;
        private Int32 m_previousSpeed;

        public ServerTemperatureController(XmlServer serverConfig)
        {
            m_ipmiServer = new IpmiServer(serverConfig);
        }

        public class UpdateResult
        {
            public enum Status
            {
                Unreachable,
                NoResult,
                NoChange,
                ChangeManualSpeed,
                ChangeToAutomatic
            }

            public Status SimpleStatus;

            public Single CurrentTemperature;
            public Int32 CurrentFanSpeed;
            public String ServerDestination;

            public UpdateResult(Int32 speed, String destination)
            {
                SimpleStatus = Status.Unreachable;
                CurrentTemperature = -274;
                CurrentFanSpeed = speed;
                ServerDestination = destination;
            }
        }

        public UpdateResult Update()
        {
            return InternalUpdate();
        }

        public async Task<UpdateResult> UpdateAsync()
        {
            return await Task.Run(() => InternalUpdate());
        }

        public void SetToAutomatic()
        {
            var ipmiController = new IPMIController(m_ipmiServer);
            ipmiController.SetAutomaticFanControl();
        }

        private UpdateResult InternalUpdate()
        {
            var ipmiController = new IPMIController(m_ipmiServer);
            var temps = ipmiController.GetTemperatures(true);

            if (String.IsNullOrWhiteSpace(temps) || temps.Contains("Error"))
            {
                m_wasPreviousSuccess = false;
                ErrorLogger.Instance.Log("ServerTemperatureController.InternalUpdate", "Get_Temperatures timed out for " + m_ipmiServer.m_address);
                return new UpdateResult(m_previousSpeed, m_ipmiServer.m_address)
                {
                    SimpleStatus = UpdateResult.Status.Unreachable
                };
            }

            var actualTemps = ipmiController.ParseServerTemps(temps);

            if (!actualTemps.IsValid())
            {
                m_wasPreviousSuccess = false;
                ipmiController.SetAutomaticFanControl();
                ErrorLogger.Instance.Log("ServerTemperatureController.InternalUpdate", "Get_Temperatures did not yield valid values for " + m_ipmiServer.m_address);
                return new UpdateResult(m_previousSpeed, m_ipmiServer.m_address)
                {
                    SimpleStatus = UpdateResult.Status.NoResult
                };
            }

            var controlTemp = Convert.ToInt32(actualTemps.Ambient);
            var setFanSpeed = m_ipmiServer.GetSpeedForTemperature(controlTemp);
            m_previousSpeed = R710SpeedConverter.FanSpeedFromIpmiSpeed(m_ipmiServer.GetSpeedForTemperature(setFanSpeed));

            var result = new UpdateResult(m_previousSpeed, m_ipmiServer.m_address)
            {
                CurrentTemperature = actualTemps.Ambient,
            };

            if (m_ipmiServer.IsManualTemperature(controlTemp))
            {
                if (!m_wasPreviousSuccess || m_previousIpmiSpeed != setFanSpeed)
                {
                    ipmiController.SetFanByServerTemperature(controlTemp, m_wasAutomatic || !m_wasPreviousSuccess);
                    result.SimpleStatus = UpdateResult.Status.ChangeManualSpeed;
                    m_wasAutomatic = false;
                    m_previousIpmiSpeed = setFanSpeed;
                }
                else
                {
                    DebugLogger.Instance.Log("ServerTemperatureController.InternalUpdate", "Temperature didn't change, did not touch fan speed.");
                    result.SimpleStatus = UpdateResult.Status.NoChange;
                }
            }
            else
            {
                DebugLogger.Instance.Log("ServerTemperatureController.InternalUpdate", "Temperature is too high, setting automatic control.");
                ipmiController.SetAutomaticFanControl();
                result.SimpleStatus = UpdateResult.Status.ChangeToAutomatic;
                m_wasAutomatic = true;
            }

            m_wasPreviousSuccess = true;
            return result;
        }
    }
}
