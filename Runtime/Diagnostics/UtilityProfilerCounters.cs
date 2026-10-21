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

		public static void BrainEnabled()
		{
			_activeBrains.Value++;
		}

		public static void BrainDisabled()
		{
			_activeBrains.Value--;
		}

		public static void SetActiveBrains(int count)
		{
			_activeBrains.Value = count;
		}

		public static void ActionSwitched()
		{
			_switches.Value++;
		}
	}
}
#endif
