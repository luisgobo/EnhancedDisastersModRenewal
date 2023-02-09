using ColossalFramework;
using ICities;
using NaturalDisastersRenewal.Logger;


namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class DisasterExtension : IDisasterBase
    {

        public override void OnCreated(IDisaster disasters)
        {
            //disasterManager = disasters;
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.OnCreated(disasters);
        }

        public override void OnDisasterStarted(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.OnDisasterStarted(disasterData.Info.m_disasterAI, disasterData.m_intensity);

            DisasterLogger.AddDisaster(Singleton<SimulationManager>.instance.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        
        public override void OnDisasterActivated(ushort disasterID) 
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.OnDisasterActivated(disasterData.Info.m_disasterAI, disasterID);
        }

        public override void OnDisasterDeactivated(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.OnDisasterDeactivated(disasterData.Info.m_disasterAI, disasterID);

            //DisasterLogger.AddDisaster(Singleton<SimulationManager>.instance.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        public override void OnDisasterDetected(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<DisasterServices.LegacyStructure.NaturalDisasterHandler>.instance.OnDisasterDetected(disasterData.Info.m_disasterAI, disasterID);

            //DisasterLogger.AddDisaster(Singleton<SimulationManager>.instance.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }

        public static void SetDisableDisasterFocus(bool disableDisasterFocus)
        {
            DebugLogger.Log("m_disableAutomaticFollow: " + disableDisasterFocus);
            DisasterManager.instance.m_disableAutomaticFollow = disableDisasterFocus;
        }

    }
}