using DTech.Blackboard;
using DTech.UtilityAI.Diagnostics;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestScoringProbe : UtilityScoringProbe
	{
		public override IUtilityContext CreateContext(RuntimeBlackboard blackboard) =>
			new TestUtilityContext(new TestUtilityAgent(), blackboard);
	}
}
