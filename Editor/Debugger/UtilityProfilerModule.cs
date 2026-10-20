using System;
using Unity.Profiling;
using Unity.Profiling.Editor;
using DTech.UtilityAI.Diagnostics;

namespace DTech.UtilityAI.Editor
{
	[Serializable]
	[ProfilerModuleMetadata("Utility AI")]
	internal class UtilityProfilerModule : ProfilerModule
	{
		private static readonly ProfilerCounterDescriptor[] _counters =
		{
			new(UtilityProfilerCounters.ActiveBrainsCounterName, ProfilerCategory.Scripts),
			new(UtilityProfilerCounters.SwitchesCounterName, ProfilerCategory.Scripts),
		};

		public UtilityProfilerModule() : base(_counters) { }
	}
}
