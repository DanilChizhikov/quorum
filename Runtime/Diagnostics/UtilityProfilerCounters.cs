#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
using Unity.Profiling;

namespace DTech.UtilityAI.Diagnostics
{
	internal static class UtilityProfilerCounters
	{
		public const string ActiveBrainsCounterName = "Utility AI Active Brains";
		public const string SwitchesCounterName = "Utility AI Switches";

		private static ProfilerCounterValue<int> _activeBrains = new(
			ProfilerCategory.Scripts,
			ActiveBrainsCounterName,
			ProfilerMarkerDataUnit.Count,
			ProfilerCounterOptions.FlushOnEndOfFrame);

		private static ProfilerCounterValue<int> _switches = new(
			ProfilerCategory.Scripts,
			SwitchesCounterName,
			ProfilerMarkerDataUnit.Count,
			ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

		public static ProfilerMarker SelectActionMarker { get; } = new("UtilityAI.SelectAction");

		/// <summary>
		/// Reads the count back from the registry instead of counting up and down: a brain collected
		/// without Disable or Dispose would otherwise inflate the counter until the domain reloads.
		/// </summary>
		public static void SyncActiveBrains()
		{
			_activeBrains.Value = UtilityBrainDebugRegistry.GetEnabledCount();
		}

		public static void ActionSwitched()
		{
			_switches.Value++;
		}
	}
}
#endif
