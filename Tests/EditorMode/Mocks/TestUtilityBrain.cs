namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestUtilityBrain : UtilityBrain<TestBrainConfig, TestUtilityContext>
	{
		public TestBrainConfig SourceConfig => Config;
		public TestUtilityContext BrainContext => Context;
		
		public TestUtilityBrain(TestBrainConfig config, TestUtilityContext context, IUtilityActionInjector injector = null)
			: base(config, context, injector) { }
	}
}
