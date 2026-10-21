using DTech.Blackboard;

namespace DTech.Quorum.Tests.EditorMode
{
	internal class TestQuorumContext : QuorumContext<TestQuorumAgent>
	{
		public TestQuorumContext(TestQuorumAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
