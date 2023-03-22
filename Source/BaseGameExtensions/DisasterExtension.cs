using ColossalFramework;
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
            Singleton<NaturalDisasterHandler>.instance.OnCreated(disasters);
        }

        public override void OnDisasterStarted(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<NaturalDisasterHandler>.instance.OnDisasterStarted(disasterData.Info.m_disasterAI, disasterData.m_intensity);

            DisasterLogger.AddDisaster(Singleton<SimulationManager>.instance.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        public override void OnDisasterActivated(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<NaturalDisasterHandler>.instance.OnDisasterActivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDeactivated(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<NaturalDisasterHandler>.instance.OnDisasterDeactivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDetected(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<NaturalDisasterHandler>.instance.OnDisasterDetected(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterFinished(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<NaturalDisasterHandler>.instance.OnDisasterFinished(disasterData.Info.m_disasterAI, disasterID);
        }

        public static void SetDisableDisasterFocus(bool disableDisasterFocus)
        {
            DisasterManager.instance.m_disableAutomaticFollow = disableDisasterFocus;
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

                            SimulationManager.instance.SimulationPaused = true;
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
            var disasterHandler = Singleton<NaturalDisasterHandler>.instance;
            if (!enabled)
            {
                DebugLogger.Log("DDS: Deactivating disaster");
                disasterHandler.GetDisasterWrapper().EndDisaster(disasterId);
            }

            return false;
        }
    }
}