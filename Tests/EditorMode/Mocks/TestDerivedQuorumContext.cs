using DTech.Blackboard;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestDerivedQuorumContext : TestQuorumContext
	{
		public TestDerivedQuorumContext(TestQuorumAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
