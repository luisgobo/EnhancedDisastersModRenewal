using ICities;
using ColossalFramework;

namespace EnhancedDisastersMod
{
    public class Threading: ThreadingExtensionBase
    {
        public override void OnAfterSimulationFrame()
        {
            // This prevent the game original random disasters to occur.
            Singleton<DisasterManager>.instance.m_randomDisasterCooldown = 0;

            // Give disasters a chance to occur
            Singleton<EnhancedDisastersManager>.instance.OnSimulationFrame();
        }
    }
}
