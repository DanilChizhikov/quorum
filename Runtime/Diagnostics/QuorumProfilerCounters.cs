#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
using Unity.Profiling;

namespace DTech.Quorum.Diagnostics
{
	internal static class QuorumProfilerCounters
	{
		public const string ActiveBrainsCounterName = "Quorum Active Brains";
		public const string SwitchesCounterName = "Quorum Switches";

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

		public static ProfilerMarker SelectActionMarker { get; } = new("Quorum.SelectAction");

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
