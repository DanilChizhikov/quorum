using DTech.Blackboard;

namespace DTech.Quorum
{
	public interface IQuorumContext
	{
		IQuorumAgent BaseAgent { get; }
		RuntimeBlackboard Blackboard { get; }
	}
}
