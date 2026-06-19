using System;
using System.Collections.Generic;
using ColossalFramework;
using NaturalDisastersRenewal.Models.NaturalDisaster;
using UnityEngine;

namespace NaturalDisastersRenewal.Common.Development
{
    public class ShelterFloodQueryService
    {
        private const float FloodedStreetDepth = 0.25f;
        private const float MeteorWaterImpactMinDepth = 1f;
        private const float MeteorWaterContactDepth = 0.01f;
        private const float MinMeteorWaterWaveReach = 2560f;
        private const float MaxMeteorWaterWaveReach = 7680f;
        private const float SimulationFramesPerSecond = 60f;

        private readonly HashSet<ulong> _clearedMeteorShelterKeys = new HashSet<ulong>();
        private readonly Dictionary<ulong, uint> _meteorImpactFrameByMeteorShelter = new Dictionary<ulong, uint>();
        private readonly Dictionary<ulong, uint> _waterContactFrameByMeteorShelter = new Dictionary<ulong, uint>();

        public bool TryGetShelterStreetFloodState(ref Building building, out ushort segmentId,
            out float waterDepth, out bool isFlooded)
        {
            segmentId = building.m_accessSegment;
            waterDepth = 0f;
            isFlooded = false;

            if (segmentId == 0)
                return false;

            var netManager = Singleton<NetManager>.instance;
            if (netManager == null)
                return false;

            var segment = netManager.m_segments.m_buffer[segmentId];
            if ((segment.m_flags & NetSegment.Flags.Created) == 0)
                return false;

            var streetPosition = GetClosestPointOnSegment(netManager, ref segment, building.m_position);
            waterDepth = GetWaterDepthAt(streetPosition);
            isFlooded = (segment.m_flags & NetSegment.Flags.Flooded) != 0 || waterDepth >= FloodedStreetDepth;
            return true;
        }

        public bool TryGetNearbyMeteorWaterImpactInfo(ushort shelterId, Vector3 shelterPosition, float waterDepth,
            out string meteorInfo)
        {
            meteorInfo = null;

            var disasterManager = Services.Disasters;
            var simulationManager = Services.Simulation;
            if (disasterManager == null || simulationManager == null)
                return false;

            var currentFrame = simulationManager.m_currentFrameIndex;
            if (waterDepth <= MeteorWaterContactDepth)
                ClearDryShelterMeteorContacts(shelterId);

            var foundMeteor = false;
            ushort bestDisasterId = 0;
            var bestDistance = float.MaxValue;
            byte bestIntensity = 0;
            var bestImpactFrame = 0u;
            var bestHasImpactFrame = false;
            var bestImpactFrameIsFallback = false;
            var bestIsActive = false;

            for (var disasterIndex = 0; disasterIndex < disasterManager.m_disasters.m_buffer.Length; disasterIndex++)
            {
                var disasterId = (ushort)disasterIndex;
                var disaster = disasterManager.m_disasters.m_buffer[disasterId];
                bool isActive;
                if (!IsTrackedMeteorStrikeWaterImpact(disasterId, ref disaster, out isActive))
                    continue;

                var impactPosition = disaster.m_targetPosition;
                var distance = GetHorizontalDistance(impactPosition, shelterPosition);
                var waveReach = GetMeteorWaterWaveReach(disaster.m_intensity);
                if (distance > waveReach)
                    continue;

                uint impactFrame;
                bool impactFrameIsFallback;
                var hasImpactFrame = TryGetMeteorImpactOrActivationFrame(
                    disasterId,
                    ref disaster,
                    currentFrame,
                    out impactFrame,
                    out impactFrameIsFallback);
                var candidateKey = GetMeteorShelterKey(disasterId, shelterId, hasImpactFrame ? impactFrame : 0u);
                if (waterDepth <= MeteorWaterContactDepth &&
                    (_clearedMeteorShelterKeys.Contains(candidateKey) || !isActive))
                    continue;

                if (foundMeteor && distance >= bestDistance)
                    continue;

                foundMeteor = true;
                bestDisasterId = disasterId;
                bestDistance = distance;
                bestIntensity = disaster.m_intensity;
                bestImpactFrame = impactFrame;
                bestHasImpactFrame = hasImpactFrame;
                bestImpactFrameIsFallback = impactFrameIsFallback;
                bestIsActive = isActive;
            }

            if (!foundMeteor)
                return false;

            var key = GetMeteorShelterKey(bestDisasterId, shelterId, bestHasImpactFrame ? bestImpactFrame : 0u);
            uint contactFrame;
            var hasContactFrame = _waterContactFrameByMeteorShelter.TryGetValue(key, out contactFrame);

            if (waterDepth <= MeteorWaterContactDepth)
            {
                if (_clearedMeteorShelterKeys.Contains(key) || !bestIsActive)
                    return false;
            }
            else
            {
                _clearedMeteorShelterKeys.Remove(key);
            }

            if (waterDepth > MeteorWaterContactDepth && !_waterContactFrameByMeteorShelter.ContainsKey(key))
            {
                _waterContactFrameByMeteorShelter[key] = currentFrame;
                if (bestHasImpactFrame)
                    _meteorImpactFrameByMeteorShelter[key] = bestImpactFrame;
            }

            hasContactFrame = _waterContactFrameByMeteorShelter.TryGetValue(key, out contactFrame);
            uint storedImpactFrame;
            if (!bestHasImpactFrame && _meteorImpactFrameByMeteorShelter.TryGetValue(key, out storedImpactFrame))
            {
                bestImpactFrame = storedImpactFrame;
                bestHasImpactFrame = true;
            }

            var impactFrameText = bestHasImpactFrame
                ? bestImpactFrame + (bestImpactFrameIsFallback ? " (base)" : string.Empty)
                : "n/d";
            var elapsedFromImpactText = "n/d";
            if (bestHasImpactFrame && currentFrame >= bestImpactFrame)
            {
                var elapsedFromImpact = currentFrame - bestImpactFrame;
                elapsedFromImpactText = string.Format(
                    "{0}f (~{1:0}s)",
                    elapsedFromImpact,
                    elapsedFromImpact / SimulationFramesPerSecond);
            }

            var contactFrameText = hasContactFrame ? contactFrame.ToString() : "pendiente";
            if (hasContactFrame && bestHasImpactFrame && contactFrame >= bestImpactFrame)
            {
                var elapsedFrames = contactFrame - bestImpactFrame;
                contactFrameText += string.Format(
                    " (+{0}f, ~{1:0}s)",
                    elapsedFrames,
                    elapsedFrames / SimulationFramesPerSecond);
            }

            meteorInfo = string.Format(
                "Meteor agua: #{0}\nImpacto fr: {1}\nDesde impacto: {2}\nAgua shelter fr: {3}\nIntensidad: {4:0.0} ({5})\nDist: {6:0.00}u",
                bestDisasterId,
                impactFrameText,
                elapsedFromImpactText,
                contactFrameText,
                bestIntensity / 10f,
                bestIntensity,
                bestDistance);

            return true;
        }

        private void ClearDryShelterMeteorContacts(ushort shelterId)
        {
            if (_waterContactFrameByMeteorShelter.Count == 0)
                return;

            var keysToClear = new List<ulong>();
            foreach (var key in _waterContactFrameByMeteorShelter.Keys)
                if (((key >> 32) & 0xffffUL) == shelterId)
                    keysToClear.Add(key);

            for (var i = 0; i < keysToClear.Count; i++)
            {
                var key = keysToClear[i];
                _waterContactFrameByMeteorShelter.Remove(key);
                _meteorImpactFrameByMeteorShelter.Remove(key);
                _clearedMeteorShelterKeys.Add(key);
            }
        }

        private static ulong GetMeteorShelterKey(ushort disasterId, ushort shelterId, uint impactFrame)
        {
            return ((ulong)disasterId << 48) | ((ulong)shelterId << 32) | impactFrame;
        }

        private static bool TryGetMeteorImpactOrActivationFrame(ushort disasterId, ref DisasterData disaster,
            uint currentFrame, out uint impactFrame, out bool isFallback)
        {
            isFallback = false;

            if (MeteorStrikeModel.TryGetWaterImpactFrame(disasterId, out impactFrame))
                return true;

            if (MeteorStrikeModel.TryGetMeteorActivationFrame(disasterId, out impactFrame))
                return true;

            if (disaster.m_startFrame > 0u)
            {
                impactFrame = disaster.m_startFrame;
                isFallback = true;
                return true;
            }

            if (currentFrame > 0u)
            {
                impactFrame = currentFrame;
                isFallback = true;
                return true;
            }

            impactFrame = 0u;
            return false;
        }

        private static bool IsTrackedMeteorStrikeWaterImpact(ushort disasterId, ref DisasterData disaster,
            out bool isActive)
        {
            isActive = false;

            if (disaster.Info == null || disaster.Info.m_disasterAI as MeteorStrikeAI == null)
                return false;

            isActive = (disaster.m_flags & (DisasterData.Flags.Emerging | DisasterData.Flags.Active |
                                            DisasterData.Flags.Clearing)) != 0;

            uint impactFrame;
            if (MeteorStrikeModel.TryGetWaterImpactFrame(disasterId, out impactFrame))
                return true;

            if (!isActive)
                return false;

            return IsWaterImpact(disaster.m_targetPosition);
        }

        private static bool IsWaterImpact(Vector3 position)
        {
            return GetWaterDepthAt(position) >= MeteorWaterImpactMinDepth;
        }

        private static float GetMeteorWaterWaveReach(byte intensity)
        {
            return Mathf.Lerp(
                MinMeteorWaterWaveReach,
                MaxMeteorWaterWaveReach,
                Mathf.Clamp01(intensity / 255f));
        }

        private static float GetHorizontalDistance(Vector3 first, Vector3 second)
        {
            var deltaX = first.x - second.x;
            var deltaZ = first.z - second.z;
            return (float)Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        }

        private static Vector3 GetClosestPointOnSegment(NetManager netManager, ref NetSegment segment, Vector3 position)
        {
            var startPosition = netManager.m_nodes.m_buffer[segment.m_startNode].m_position;
            var endPosition = netManager.m_nodes.m_buffer[segment.m_endNode].m_position;
            var segmentVector = endPosition - startPosition;
            var segmentLengthSqr = segmentVector.sqrMagnitude;

            if (segmentLengthSqr <= 0.01f)
                return segment.m_middlePosition;

            var t = Mathf.Clamp01(Vector3.Dot(position - startPosition, segmentVector) / segmentLengthSqr);
            return startPosition + segmentVector * t;
        }

        private static float GetWaterDepthAt(Vector3 position)
        {
            var terrain = Services.Terrain;
            if (terrain == null)
                return 0f;

            var terrainHeight = terrain.SampleRawHeightSmooth(position);
            var waterSurfaceHeight = terrain.SampleRawHeightSmoothWithWater(position, false, 0f);
            return Mathf.Max(0f, waterSurfaceHeight - terrainHeight);
        }
    }
}
