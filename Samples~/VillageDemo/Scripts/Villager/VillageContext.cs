using DTech.Blackboard;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// What every villager action and consideration receives: the agent plus its own blackboard.
	/// </summary>
	public sealed class VillageContext : QuorumContext<VillagerAgent>
	{
		public VillageContext(VillagerAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
