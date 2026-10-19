using DTech.Blackboard;

namespace DTech.UtilityAI
{
	public interface IUtilityContext
	{
		IUtilityAgent BaseAgent { get; }
		RuntimeBlackboard Blackboard { get; }
	}
}
