using DTech.Blackboard;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestOtherUtilityContext : UtilityContext<TestUtilityAgent>
	{
		public TestOtherUtilityContext(TestUtilityAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
