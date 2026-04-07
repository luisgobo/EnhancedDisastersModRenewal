using ICities;
using CommonServices = NaturalDisastersRenewal.Common.Services;

namespace NaturalDisastersRenewal.BaseGameExtensions
{
    public class Threading : ThreadingExtensionBase
    {
        public override void OnAfterSimulationFrame()
        {
            // This prevent the game original random disasters to occur.
            CommonServices.Disasters.m_randomDisasterCooldown = 0;

            // Give disasters a chance to occur
            CommonServices.DisasterHandler.OnSimulationFrame();
        }
    }
}