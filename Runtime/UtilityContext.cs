namespace DTech.UtilityAI
{
	public abstract class UtilityContext<TAgent> : IUtilityContext
		where TAgent : IUtilityAgent
	{
		public IUtilityAgent BaseAgent => Agent;
		public TAgent Agent { get; }

		public UtilityContext(TAgent agent)
		{
			Agent = agent;
		}
	}
}