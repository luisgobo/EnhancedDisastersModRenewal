using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;

namespace EnhancedDisastersMod
{
    public class EnhancedTornado : EnhancedDisaster
    {
        public class Data : SerializableDataCommon, IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                EnhancedTornado d = Singleton<EnhancedDisastersManager>.instance.container.Tornado;
                serializeCommonParameters(s, d);
                s.WriteInt32(d.MaxProbabilityMonth);
                s.WriteBool(d.NoTornadoDuringFog);
            }

            public void Deserialize(DataSerializer s)
            {
                EnhancedTornado d = Singleton<EnhancedDisastersManager>.instance.container.Tornado;
                deserializeCommonParameters(s, d);

                if (s.version >= 3)
                {
                    d.MaxProbabilityMonth = s.ReadInt32();
                }
                d.NoTornadoDuringFog = s.ReadBool();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                afterDeserializeLog("Tornado");
            }
        }

        public int MaxProbabilityMonth = 5;
        public bool NoTornadoDuringFog = true;

        public EnhancedTornado()
        {
            DType = DisasterType.Tornado;
            BaseOccurrencePerYear = 1.5f;
            ProbabilityDistribution = ProbabilityDistributions.PowerLow;

            calmDays = 360 * 2;
            probabilityWarmupDays = 180;
            intensityWarmupDays = 180;
        }

        protected override float getCurrentOccurrencePerYear_local()
        {
            if (NoTornadoDuringFog && Singleton<WeatherManager>.instance.m_currentFog > 0)
            {
                return 0;
            }

            DateTime dt = Singleton<SimulationManager>.instance.m_currentGameTime;
            int delta_month = Math.Abs(dt.Month - MaxProbabilityMonth);
            if (delta_month > 6) delta_month = 12 - delta_month;

            float occurrence = base.getCurrentOccurrencePerYear_local() * (1f - delta_month / 6f);

            return occurrence;
        }

        public override string GetProbabilityTooltip()
        {
            if (calmDaysLeft <= 0)
            {
                if (NoTornadoDuringFog && Singleton<WeatherManager>.instance.m_currentFog > 0)
                {
                    return "No " + GetName() + " during fog.";
                }
            }

            return base.GetProbabilityTooltip();
        }

        public override bool CheckDisasterAIType(object disasterAI)
        {
            return disasterAI as TornadoAI != null;
        }

        public override string GetName()
        {
            return "Tornado";
        }

        public override void CopySettings(EnhancedDisaster disaster)
        {
            base.CopySettings(disaster);

            EnhancedTornado d = disaster as EnhancedTornado;
            if (d != null)
            {
                MaxProbabilityMonth = d.MaxProbabilityMonth;
                NoTornadoDuringFog = d.NoTornadoDuringFog;
            }
        }
    }
}