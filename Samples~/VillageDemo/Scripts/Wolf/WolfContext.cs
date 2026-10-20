using DTech.Blackboard;

namespace DTech.UtilityAI.Samples.Village
{
	public sealed class WolfContext : UtilityContext<WolfAgent>
	{
		public WolfContext(WolfAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
