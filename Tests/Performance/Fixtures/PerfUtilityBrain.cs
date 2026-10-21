namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfUtilityBrain : UtilityBrain<PerfBrainConfig, PerfUtilityContext>
	{
		public PerfUtilityBrain(PerfBrainConfig config, PerfUtilityContext context) : base(config, context) { }
	}
}
