using DTech.Blackboard;

namespace DTech.Quorum
{
	public abstract class QuorumContext<TAgent> : IQuorumContext
		where TAgent : IQuorumAgent
	{
		public IQuorumAgent BaseAgent => Agent;
		public RuntimeBlackboard Blackboard { get; }
		public TAgent Agent { get; }

		public QuorumContext(TAgent agent, RuntimeBlackboard blackboard)
		{
			Agent = agent;
			Blackboard = blackboard;
		}
	}
}