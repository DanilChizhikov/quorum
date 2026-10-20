namespace DTech.UtilityAI.Samples.Village
{
	public sealed class WolfBrain : UtilityBrain<WolfBrainConfig, WolfContext>
	{
		public WolfBrain(WolfBrainConfig config, WolfContext context, IUtilityActionInjector injector)
			: base(config, context, injector) { }
	}
}
