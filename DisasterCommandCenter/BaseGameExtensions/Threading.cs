using ColossalFramework;
using NaturalDisastersRenewal.Common;
using ICities;
using NaturalDisastersRenewal.Handlers;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class Threading : ThreadingExtensionBase
    {
        public override void OnAfterSimulationFrame()
        {
            // This prevent the game original random disasters to occur.
            Services.Disasters.m_randomDisasterCooldown = 0;

            // Give disasters a chance to occur
            Services.DisasterHandler.OnSimulationFrame();
        }
    }
}