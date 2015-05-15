using ICities;

namespace MultiTrackStationEnabler
{
	public class LoadingExtension : LoadingExtensionBase
	{
		public override void OnLevelLoaded (LoadMode mode)
		{
			RedirectionHelper.RedirectCalls (typeof(TransportTool), typeof(FakeTransportTool), "GetStopPosition", true);
		}
	}
}

