using ColossalFramework;
using ICities;

namespace EnhancedDisastersMod
{
    public class DisasterExtension : IDisasterBase
    {
        public override void OnDisasterStarted(ushort disasterID)
        {
            DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disasterID];
            Singleton<EnhancedDisastersManager>.instance.OnDisasterStarted(disasterData.Info.m_disasterAI, disasterData.m_intensity);

            DisasterLogger.AddDisaster(Singleton<SimulationManager>.instance.m_currentGameTime, disasterData.Info.GetAI().name, disasterData.m_intensity);
        }
    }
}