using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace MultiTrackStationEnabler
{
	public class FakeTransportTool
	{
		private bool GetStopPosition(TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
		{
			bool alternateMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			fixedPlatform = false;
			if (segment != 0)
			{
				NetManager instance = Singleton<NetManager>.instance;
				if (!alternateMode && (instance.m_segments.m_buffer [(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
				{
					building = NetSegment.FindOwnerBuilding (segment, 363f);
					if (building != 0)
					{
						BuildingManager instance3 = Singleton<BuildingManager>.instance;
						BuildingInfo info3 = instance3.m_buildings.m_buffer[(int)building].Info;
						TransportInfo transportLineInfo = info3.m_buildingAI.GetTransportLineInfo ();
						if (transportLineInfo != null && transportLineInfo.m_transportType == info.m_transportType)
						{
							segment = 0;
						}
						else
						{
							building = 0;
						}
					}
				}
				Vector3 point;
				int num;
				float num2;
				Vector3 vector;
				int num3;
				float num4;
				if (segment != 0 && instance.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out point, out num, out num2) && instance.m_segments.m_buffer[(int)segment].GetClosestLanePosition(point, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out vector, out num3, out num4))
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
						Vector3 vector3 = Vector3.zero;
						int num6 = 1000000;
						for (int i = 0; i < 12; i++)
						{
							Randomizer randomizer = new Randomizer(i);
							Vector3 vector4;
							Vector3 a;
							info2.m_buildingAI.CalculateSpawnPosition(building, ref instance2.m_buildings.m_buffer[(int)building], ref randomizer, randomVehicleInfo, out vector4, out a);
							int lineCount = this.GetLineCount(vector4, a - vector4, info.m_transportType);
							if (lineCount < num6)
							{
								vector3 = vector4;
								num6 = lineCount;
							}
						}
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

		private int GetLineCount(Vector3 stopPosition, Vector3 stopDirection, TransportInfo.TransportType transportType)
		{
			NetManager instance = Singleton<NetManager>.instance;
			TransportManager instance2 = Singleton<TransportManager>.instance;
			stopDirection.Normalize();
			Segment3 segment = new Segment3(stopPosition - stopDirection * 16, stopPosition + stopDirection * 16);
			Vector3 vector = segment.Min();
			Vector3 vector2 = segment.Max();
			int num = Mathf.Max((int)((vector.x - 4) / 64 + 135), 0);
			int num2 = Mathf.Max((int)((vector.z - 4) / 64 + 135), 0);
			int num3 = Mathf.Min((int)((vector2.x + 4) / 64 + 135), 269);
			int num4 = Mathf.Min((int)((vector2.z + 4) / 64 + 135), 269);
			int num5 = 0;
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num; j <= num3; j++)
				{
					ushort num6 = instance.m_nodeGrid[i * 270 + j];
					int num7 = 0;
					while (num6 != 0)
					{
						ushort transportLine = instance.m_nodes.m_buffer[(int)num6].m_transportLine;
						if (transportLine != 0)
						{
							TransportInfo info = instance2.m_lines.m_buffer[(int)transportLine].Info;
							if (info.m_transportType == transportType && (instance2.m_lines.m_buffer[(int)transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None && segment.DistanceSqr(instance.m_nodes.m_buffer[(int)num6].m_position) < 16)
							{
								num5++;
							}
						}
						num6 = instance.m_nodes.m_buffer[(int)num6].m_nextGridNode;
						if (++num7 >= 32768)
						{
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!" + Environment.StackTrace);
							break;
						}
					}
				}
			}
			return num5;
		}
	}
}

