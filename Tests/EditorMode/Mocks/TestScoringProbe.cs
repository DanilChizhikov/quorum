using DTech.Blackboard;
using DTech.Quorum.Diagnostics;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestScoringProbe : ScoringProbe
	{
		public override IQuorumContext CreateContext(RuntimeBlackboard blackboard) =>
			new TestQuorumContext(new TestQuorumAgent(), blackboard);
	}
}
