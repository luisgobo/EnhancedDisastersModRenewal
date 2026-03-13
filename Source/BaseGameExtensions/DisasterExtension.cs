using System;
using System.Threading;
using ICities;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class DisasterExtension : IDisasterBase
    {
        public override void OnCreated(IDisaster disasters)
        {
            CommonServices.DisasterHandler.OnCreated(disasters);
        }

        public override void OnDisasterStarted(ushort disasterID)
        {
            var disasterData = CommonServices.Disasters.m_disasters.m_buffer[disasterID];
            CommonServices.DisasterHandler.OnDisasterStarted(disasterData.Info.m_disasterAI, disasterData.m_intensity);

            DisasterLogger.AddDisaster(CommonServices.Simulation.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        public override void OnDisasterActivated(ushort disasterID)
        {
            var disasterData = CommonServices.Disasters.m_disasters.m_buffer[disasterID];
            CommonServices.DisasterHandler.OnDisasterActivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDeactivated(ushort disasterID)
        {
            var disasterData = CommonServices.Disasters.m_disasters.m_buffer[disasterID];
            CommonServices.DisasterHandler.OnDisasterDeactivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDetected(ushort disasterID)
        {
            var disasterData = CommonServices.Disasters.m_disasters.m_buffer[disasterID];
            CommonServices.DisasterHandler.OnDisasterDetected(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterFinished(ushort disasterID)
        {
            var disasterData = CommonServices.Disasters.m_disasters.m_buffer[disasterID];
            CommonServices.DisasterHandler.OnDisasterFinished(disasterData.Info.m_disasterAI, disasterID);
        }

        public static void SetDisableDisasterFocus(bool disableDisasterFocus)
        {
            CommonServices.Disasters.m_disableAutomaticFollow = disableDisasterFocus;
        }

        public static void SetPauseOnDisasterStarts(bool disablePause, double secondsBeforePausing, ushort disasterId, DisasterSettings disasterInfo, bool enabled)
        {
            //Pause when disaster start
            if (TryDisableDisaster(disasterId, enabled))
            {
                return;
            }

            if (disablePause)
            {
                new Thread(
                    () =>
                    {
                        try
                        {
                            var pauseStart = DateTime.UtcNow + TimeSpan.FromSeconds(-secondsBeforePausing);

                            while (DateTime.UtcNow < pauseStart) { }

                            CommonServices.Simulation.SimulationPaused = true;
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.Log(ex.ToString());

                            throw;
                        }
                    }).Start();
            }
        }

        private static bool TryDisableDisaster(ushort disasterId, bool enabled)
        {
            var disasterHandler = CommonServices.DisasterHandler;
            if (!enabled)
            {
                DebugLogger.Log("DDS: Deactivating disaster");
                disasterHandler.GetDisasterWrapper().EndDisaster(disasterId);
            }

            return false;
        }
    }
}