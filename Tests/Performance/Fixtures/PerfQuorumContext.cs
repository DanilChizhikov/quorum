using DTech.Blackboard;

namespace DTech.Quorum.Tests.Performance
{
	internal sealed class PerfQuorumContext : QuorumContext<PerfQuorumAgent>
	{
		public PerfQuorumContext(PerfQuorumAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
