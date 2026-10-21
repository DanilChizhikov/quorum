namespace DTech.Quorum.Samples.Village
{
	public sealed class VillagerBrain : QuorumBrain<VillagerBrainConfig, VillageContext>
	{
		public VillagerBrain(VillagerBrainConfig config, VillageContext context, IQuorumActionInjector injector)
			: base(config, context, injector) { }
	}
}
