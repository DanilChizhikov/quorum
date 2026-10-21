using DTech.Blackboard;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal class TestUtilityContext : UtilityContext<TestUtilityAgent>
	{
		public TestUtilityContext(TestUtilityAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
