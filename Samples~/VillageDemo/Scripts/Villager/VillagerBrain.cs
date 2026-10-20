namespace DTech.UtilityAI.Samples.Village
{
	public sealed class VillagerBrain : UtilityBrain<VillagerBrainConfig, VillageContext>
	{
		public VillagerBrain(VillagerBrainConfig config, VillageContext context, IUtilityActionInjector injector)
			: base(config, context, injector) { }
	}
}
