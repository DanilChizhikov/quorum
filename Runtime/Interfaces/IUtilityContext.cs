using UnityEditor.Experimental.GraphView;

namespace DTech.UtilityAI
{
	public interface IUtilityContext
	{
		IUtilityAgent BaseAgent { get; }
		Blackboard Blackboard { get; }
	}
}
