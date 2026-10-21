namespace DTech.Quorum.Tests.Performance
{
	internal readonly struct PerfBrainSetup
	{
		public int BrainCount { get; }
		public int ActionCount { get; }
		public int ConsiderationCount { get; }
		public PerfConsiderationKind ConsiderationKind { get; }
		public PerfActionKind ActionKind { get; }

		public PerfBrainSetup(
			int brainCount,
			int actionCount,
			int considerationCount,
			PerfConsiderationKind considerationKind,
			PerfActionKind actionKind = PerfActionKind.Inline)
		{
			BrainCount = brainCount;
			ActionCount = actionCount;
			ConsiderationCount = considerationCount;
			ConsiderationKind = considerationKind;
			ActionKind = actionKind;
		}
	}
}
