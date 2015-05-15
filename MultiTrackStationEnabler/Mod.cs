using ICities;

namespace MultiTrackStationEnabler
{
	public class Mod : IUserMod
	{
		public string Name { get { return "Multi-Track Station Enabler"; } }
		public string Description { get { return "Hold Shift when placing train stops to disable snapping to a station's spawn point."; } }
	}
}

