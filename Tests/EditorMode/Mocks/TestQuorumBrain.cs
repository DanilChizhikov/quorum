namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestQuorumBrain : QuorumBrain<TestBrainConfig, TestQuorumContext>
	{
		public TestBrainConfig SourceConfig => Config;
		public TestQuorumContext BrainContext => Context;
		
		public TestQuorumBrain(TestBrainConfig config, TestQuorumContext context, IQuorumActionInjector injector = null)
			: base(config, context, injector) { }
	}
}
