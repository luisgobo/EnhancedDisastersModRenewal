using System.Text;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Handlers
{
    public static class DisasterActionsService
    {
        public static void StopAllDisasters()
        {
            var sb = new StringBuilder();
            var vm = Services.Vehicles;
            for (var i = 1; i < 16384; i++)
            {
                if ((vm.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Created) == 0) continue;
                if (vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is MeteorAI ||
                    vm.m_vehicles.m_buffer[i].Info.m_vehicleAI is VortexAI)
                    vm.ReleaseVehicle((ushort)i);
            }

            var ws = Services.Water;
            if (ws != null)
                for (var i = ws.m_waterWaves.m_size; i >= 1; i--)
                    Services.Terrain.WaterSimulation.ReleaseWaterWave((ushort)i);

            var dm = Services.Disasters;
            var disasterWrapper = Services.DisasterHandler.GetDisasterWrapper();
            for (var i = 0; i < dm.m_disasters.m_buffer.Length; i++)
            {
                var flags = dm.m_disasters.m_buffer[i].m_flags;
                if ((flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing)) ==
                    DisasterData.Flags.None)
                    continue;

                var disasterInfo = dm.m_disasters.m_buffer[i].Info;
                if (disasterInfo == null)
                    continue;

                sb.AppendLine(disasterInfo.name + " flags: " + flags);
                if (!IsStoppableDisaster(disasterInfo.m_disasterAI))
                    continue;

                var disasterId = (ushort)i;
                if (disasterWrapper != null)
                {
                    disasterWrapper.EndDisaster(disasterId);
                    continue;
                }

                dm.m_disasters.m_buffer[disasterId].m_flags =
                    (flags & ~(DisasterData.Flags.Emerging | DisasterData.Flags.Active | DisasterData.Flags.Clearing))
                    | DisasterData.Flags.Finished;
            }

            if (Services.DisasterHandler.container.ActiveDisasters != null)
                Services.DisasterHandler.container.ActiveDisasters.Clear();

            Debug.Log(sb.ToString());
        }

        public static void ResetAllDisasterProgress()
        {
            for (var i = 0; i < Services.DisasterHandler.container.AllDisasters.Count; i++)
            {
                var disaster = Services.DisasterHandler.container.AllDisasters[i];
                disaster.ResetProbabilityProgress();
            }
        }

        public static void ToggleDisasterEnabled(DisasterBaseModel disaster)
        {
            disaster.SetEnabled(!disaster.Enabled);
        }

        private static bool IsStoppableDisaster(DisasterAI ai)
        {
            return ai as ThunderStormAI != null || ai as SinkholeAI != null || ai as TornadoAI != null ||
                   ai as EarthquakeAI != null || ai as MeteorStrikeAI != null || ai as ForestFireAI != null ||
                   ai as TsunamiAI != null;
        }
    }
}
