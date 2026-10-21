namespace DTech.Quorum.Tests.Performance
{
	internal sealed class PerfQuorumBrain : QuorumBrain<PerfBrainConfig, PerfQuorumContext>
	{
		public PerfBrainConfig SourceConfig => Config;
		public PerfQuorumContext BrainContext => Context;

		public PerfQuorumBrain(PerfBrainConfig config, PerfQuorumContext context) : base(config, context) { }
	}
}
