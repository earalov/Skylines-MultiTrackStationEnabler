using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace MultiTrackStationEnabler
{
	public class FakeTransportTool
	{
		public bool GetStopPosition(TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
		{
			bool alternateMode = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
			fixedPlatform = false;
			if (segment != 0)
			{
				NetManager instance = Singleton<NetManager>.instance;
				if (!alternateMode && (instance.m_segments.m_buffer [(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
				{
					building = NetSegment.FindOwnerBuilding (segment, 363f);
					if (building != 0)
					{
						segment = 0;
					}
				}
				Vector3 point;
				int num;
				float num2;
				Vector3 vector;
				int num3;
				float num4;
				if (segment != 0 && instance.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out point, out num, out num2) && instance.m_segments.m_buffer[(int)segment].GetClosestLanePosition(point, NetInfo.LaneType.Vehicle, info.m_vehicleType, out vector, out num3, out num4))
				{
					PathUnit.Position pathPos;
					pathPos.m_segment = segment;
					pathPos.m_lane = (byte)num3;
					pathPos.m_offset = 128;
					NetInfo.Lane lane = instance.m_segments.m_buffer[(int)segment].Info.m_lanes[num3];
					if (!lane.m_allowStop)
					{
						return false;
					}
					float num5 = lane.m_stopOffset;
					if ((instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
					{
						num5 = -num5;
					}
					uint laneID = PathManager.GetLaneID(pathPos);
					Vector3 vector2;
					instance.m_lanes.m_buffer[(int)((UIntPtr)laneID)].CalculateStopPositionAndDirection((float)pathPos.m_offset * 0.003921569f, num5, out hitPos, out vector2);
					fixedPlatform = true;
					return true;
				}
			}
			if (!alternateMode && building != 0)
			{
				VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
				if (randomVehicleInfo != null)
				{
					BuildingManager instance2 = Singleton<BuildingManager>.instance;
					BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)building].Info;
					if (info2.m_buildingAI.GetTransportLineInfo() != null)
					{
						Randomizer randomizer = new Randomizer((int)building);
						Vector3 vector3;
						Vector3 vector4;
						info2.m_buildingAI.CalculateSpawnPosition(building, ref instance2.m_buildings.m_buffer[(int)building], ref randomizer, randomVehicleInfo, out vector3, out vector4);
						if (firstStop != 0)
						{
							Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)firstStop].m_position;
							if (Vector3.SqrMagnitude(position - vector3) < 16384f && instance2.FindBuilding(vector3, 128f, info.m_class.m_service, info.m_class.m_subService, Building.Flags.None, Building.Flags.None) == building)
							{
								hitPos = position;
								return true;
							}
						}
						hitPos = vector3;
						return true;
					}
				}
			}
			return false;
		}
	}
}

