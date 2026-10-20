using DTech.Blackboard;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// What every villager action and consideration receives: the agent plus its own blackboard.
	/// </summary>
	public sealed class VillageContext : UtilityContext<VillagerAgent>
	{
		public VillageContext(VillagerAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
