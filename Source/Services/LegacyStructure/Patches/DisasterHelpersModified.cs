using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;

namespace NaturalDisastersRenewal.DisasterServices.LegacyStructure.Patches
{
    static class DisasterHelpersModified
    {
        public static void DestroyBuildings(int seed, InstanceManager.Group group, Vector3 position, float preRadius, float removeRadius,
            float destructionRadiusMin, float destructionRadiusMax, float burnRadiusMin, float burnRadiusMax, float probability)
        {
            int num = Mathf.Max((int)((position.x - preRadius - 72f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((position.z - preRadius - 72f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((position.x + preRadius + 72f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((position.z + preRadius + 72f) / 64f + 135f), 269);
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        ushort nextGridBuilding = buildings.m_buffer[(int)num5].m_nextGridBuilding;
                        Building.Flags flags = buildings.m_buffer[(int)num5].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Untouchable | Building.Flags.Demolishing)) == Building.Flags.Created)
                        {
                            Vector3 position2 = buildings.m_buffer[(int)num5].m_position;
                            float num7 = VectorUtils.LengthXZ(position2 - position);
                            if (num7 < preRadius)
                            {
                                Randomizer randomizer = new Randomizer((int)num5 | seed << 16);
                                float num8 = (destructionRadiusMax - num7) / Mathf.Max(1f, destructionRadiusMax - destructionRadiusMin);
                                float num9 = (burnRadiusMax - num7) / Mathf.Max(1f, burnRadiusMax - burnRadiusMin);
                                bool flag = false;
                                bool flag2 = (float)randomizer.Int32(10000u) < num8 * probability * 10000f;
                                bool flag3 = (float)randomizer.Int32(10000u) < num9 * probability * 10000f;
                                if (removeRadius != 0f && num7 - 72f < removeRadius)
                                {
                                    BuildingInfo info = buildings.m_buffer[(int)num5].Info;
                                    if (info.m_circular)
                                    {
                                        float num10 = (float)(buildings.m_buffer[(int)num5].Width + buildings.m_buffer[(int)num5].Length) * 2f;
                                        flag = (num7 < removeRadius + num10);
                                    }
                                    else
                                    {
                                        float angle = buildings.m_buffer[(int)num5].m_angle;
                                        Vector2 a = VectorUtils.XZ(position2);
                                        Vector2 vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                                        Vector2 vector2 = new Vector2(vector.y, -vector.x);
                                        vector *= (float)buildings.m_buffer[(int)num5].Width * 4f;
                                        vector2 *= (float)buildings.m_buffer[(int)num5].Length * 4f;
                                        Quad2 quad = default(Quad2);
                                        quad.a = a - vector - vector2;
                                        quad.b = a + vector - vector2;
                                        quad.c = a + vector + vector2;
                                        quad.d = a - vector + vector2;
                                        float num11 = VectorUtils.LengthXZ(position - new Vector3(quad.a.x, position2.y, quad.a.y));
                                        float num12 = VectorUtils.LengthXZ(position - new Vector3(quad.b.x, position2.y, quad.b.y));
                                        float num13 = VectorUtils.LengthXZ(position - new Vector3(quad.c.x, position2.y, quad.c.y));
                                        float num14 = VectorUtils.LengthXZ(position - new Vector3(quad.d.x, position2.y, quad.d.y));
                                        flag = (quad.Intersect(VectorUtils.XZ(position)) || num11 < removeRadius || num12 < removeRadius || num13 < removeRadius || num14 < removeRadius);
                                    }
                                }
                                if (flag)
                                {
                                    BuildingInfo info2 = buildings.m_buffer[(int)num5].Info;
                                    info2.m_buildingAI.CollapseBuilding(num5, ref buildings.m_buffer[(int)num5], group, false, true, Mathf.RoundToInt(num9 * 255f));
                                }
                                else if (flag2)
                                {
                                    BuildingInfo info3 = buildings.m_buffer[(int)num5].Info;
                                    ItemClass.Level lvl = (ItemClass.Level)buildings.m_buffer[(int)num5].m_level;

                                    float p = 0.5f;

                                    if (info3.m_buildingAI as OfficeBuildingAI != null || info3.m_buildingAI as CommercialBuildingAI != null || info3.m_buildingAI as IndustrialBuildingAI != null)
                                    {
                                        switch (lvl)
                                        {
                                            case ItemClass.Level.Level1:
                                                p = 1f;
                                                break;

                                            case ItemClass.Level.Level2:
                                                p = 0.6f;
                                                break;

                                            case ItemClass.Level.Level3:
                                                p = 0.2f;
                                                break;
                                        }
                                    }
                                    else if (info3.m_buildingAI as ResidentialBuildingAI != null)
                                    {
                                        switch (lvl)
                                        {
                                            case ItemClass.Level.Level1:
                                                p = 1f;
                                                break;

                                            case ItemClass.Level.Level2:
                                                p = 0.8f;
                                                break;

                                            case ItemClass.Level.Level3:
                                                p = 0.6f;
                                                break;

                                            case ItemClass.Level.Level4:
                                                p = 0.4f;
                                                break;

                                            case ItemClass.Level.Level5:
                                                p = 0.2f;
                                                break;
                                        }
                                    }

                                    // Large buildings are tougher
                                    float s = Mathf.Sqrt(buildings.m_buffer[(int)num5].Length * buildings.m_buffer[(int)num5].Width);
                                    if (s > 4)
                                    {
                                        p -= s / 16;
                                    }

                                    // Make shelters a little more useful
                                    if ((flags & Building.Flags.Evacuating) == Building.Flags.Evacuating)
                                    {
                                        p -= 0.2f;
                                    }

                                    if (p > 0 && (float)randomizer.Int32(10000u) < p * 10000f)
                                    {
                                        //Debug.Log("Destroyed: " + info3.name + " (" + info3.m_buildingAI.name + "), level: " + lvl.ToString());
                                        info3.m_buildingAI.CollapseBuilding(num5, ref buildings.m_buffer[(int)num5], group, false, false, Mathf.RoundToInt(num9 * 255f));
                                    }
                                }
                                else if (flag3 && (flags & Building.Flags.Collapsed) == Building.Flags.None && buildings.m_buffer[(int)num5].m_fireIntensity == 0)
                                {
                                    BuildingInfo info4 = buildings.m_buffer[(int)num5].Info;
                                    info4.m_buildingAI.BurnBuilding(num5, ref buildings.m_buffer[(int)num5], group, false);
                                }
                            }
                        }
                        num5 = nextGridBuilding;
                        if (++num6 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }
    }
}