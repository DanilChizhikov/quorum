using DTech.Blackboard;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestDerivedUtilityContext : TestUtilityContext
	{
		public TestDerivedUtilityContext(TestUtilityAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
	}
}
