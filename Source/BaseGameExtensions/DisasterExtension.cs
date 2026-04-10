using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ICities;
using NaturalDisastersRenewal.Handlers;
using NaturalDisastersRenewal.Logger;
using NaturalDisastersRenewal.Models.Disaster;
using System;
using System.Threading;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class DisasterExtension : IDisasterBase
    {
        public override void OnCreated(IDisaster disasters)
        {
            Services.DisasterHandler.OnCreated(disasters);
        }

        public override void OnDisasterStarted(ushort disasterID)
        {
            DisasterData disasterData = Services.Disasters.m_disasters.m_buffer[disasterID];
            Services.DisasterHandler.OnDisasterStarted(disasterData.Info.m_disasterAI, disasterData.m_intensity);

            DisasterLogger.AddDisaster(Services.Simulation.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        public override void OnDisasterActivated(ushort disasterID)
        {
            DisasterData disasterData = Services.Disasters.m_disasters.m_buffer[disasterID];
            Services.DisasterHandler.OnDisasterActivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDeactivated(ushort disasterID)
        {
            DisasterData disasterData = Services.Disasters.m_disasters.m_buffer[disasterID];
            Services.DisasterHandler.OnDisasterDeactivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDetected(ushort disasterID)
        {
            DisasterData disasterData = Services.Disasters.m_disasters.m_buffer[disasterID];
            Services.DisasterHandler.OnDisasterDetected(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterFinished(ushort disasterID)
        {
            DisasterData disasterData = Services.Disasters.m_disasters.m_buffer[disasterID];
            Services.DisasterHandler.OnDisasterFinished(disasterData.Info.m_disasterAI, disasterID);
        }

        public static void SetDisableDisasterFocus(bool disableDisasterFocus)
        {
            Services.Disasters.m_disableAutomaticFollow = disableDisasterFocus;
        }

        public static void SetPauseOnDisasterStarts(bool disablePause, double secondsBeforePausing, ushort disasterId, DisasterSettings disasterInfo, bool enabled)
        {
            //Pause when disaster start
            if (TryDisableDisaster(disasterId, disasterInfo, enabled))
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

                            Services.Simulation.SimulationPaused = true;
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.Log(ex.ToString());

                            throw;
                        }
                    }).Start();
            }
        }

        static bool TryDisableDisaster(ushort disasterId, DisasterSettings disasterInfo, bool enabled)
        {
            var disasterHandler = Services.DisasterHandler;
            if (!enabled)
            {
                DebugLogger.Log("DDS: Deactivating disaster");
                disasterHandler.GetDisasterWrapper().EndDisaster(disasterId);
            }

            return false;
        }
    }
}
