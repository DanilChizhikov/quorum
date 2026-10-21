using DTech.Blackboard;

namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfUtilityContext : UtilityContext<PerfUtilityAgent>
	{
		public PerfUtilityContext(PerfUtilityAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
