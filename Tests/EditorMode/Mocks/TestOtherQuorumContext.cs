using DTech.Blackboard;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestOtherQuorumContext : QuorumContext<TestQuorumAgent>
	{
		public TestOtherQuorumContext(TestQuorumAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
