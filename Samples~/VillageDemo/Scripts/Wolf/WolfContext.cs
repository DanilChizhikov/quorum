using DTech.Blackboard;

namespace DTech.Quorum.Samples.Village
{
	public sealed class WolfContext : QuorumContext<WolfAgent>
	{
		public WolfContext(WolfAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
