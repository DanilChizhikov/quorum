namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfUtilityBrain : UtilityBrain<PerfBrainConfig, PerfUtilityContext>
	{
		public PerfBrainConfig RuntimeConfig => Config;
		public PerfUtilityContext BrainContext => Context;

		public PerfUtilityBrain(PerfBrainConfig config, PerfUtilityContext context) : base(config, context) { }
	}
}
