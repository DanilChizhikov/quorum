using DTech.Blackboard;

namespace DTech.UtilityAI
{
	public abstract class UtilityContext<TAgent> : IUtilityContext
		where TAgent : IUtilityAgent
	{
		public IUtilityAgent BaseAgent => Agent;
		public RuntimeBlackboard Blackboard { get; }
		public TAgent Agent { get; }

		public UtilityContext(TAgent agent, RuntimeBlackboard blackboard)
		{
			Agent = agent;
			Blackboard = blackboard;
		}
	}
}