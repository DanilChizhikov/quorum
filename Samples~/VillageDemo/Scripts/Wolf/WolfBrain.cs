namespace DTech.Quorum.Samples.Village
{
	public sealed class WolfBrain : QuorumBrain<WolfBrainConfig, WolfContext>
	{
		public WolfBrain(WolfBrainConfig config, WolfContext context, IQuorumActionInjector injector)
			: base(config, context, injector) { }
	}
}
