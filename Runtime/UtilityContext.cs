using UnityEditor.Experimental.GraphView;

namespace DTech.UtilityAI
{
	public abstract class UtilityContext<TAgent> : IUtilityContext
		where TAgent : IUtilityAgent
	{
		public IUtilityAgent BaseAgent => Agent;
		public Blackboard Blackboard { get; }
		public TAgent Agent { get; }

		public UtilityContext(TAgent agent, Blackboard blackboard)
		{
			Agent = agent;
			Blackboard = blackboard;
		}
	}
}