namespace DTech.UtilityAI
{
	public abstract class UtilityContext<TAgent> : IUtilityContext
		where TAgent : IUtilityAgent
	{
		public IUtilityAgent BaseAgent => Agent;
		public Blackboard.RuntimeBlackboard Blackboard { get; }
		public TAgent Agent { get; }

		public UtilityContext(TAgent agent, Blackboard.RuntimeBlackboard blackboard)
		{
			Agent = agent;
			Blackboard = blackboard;
		}
	}
}