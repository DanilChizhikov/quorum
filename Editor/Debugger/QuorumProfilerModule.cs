using System;
using Unity.Profiling;
using Unity.Profiling.Editor;
using DTech.Quorum.Diagnostics;

namespace DTech.Quorum.Editor
{
	[Serializable]
	[ProfilerModuleMetadata("Quorum")]
	internal class QuorumProfilerModule : ProfilerModule
	{
		private static readonly ProfilerCounterDescriptor[] _counters =
		{
			new(QuorumProfilerCounters.ActiveBrainsCounterName, ProfilerCategory.Scripts),
			new(QuorumProfilerCounters.SwitchesCounterName, ProfilerCategory.Scripts),
		};

		public QuorumProfilerModule() : base(_counters) { }
	}
}
