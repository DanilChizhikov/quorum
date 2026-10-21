using System;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestOtherContextQuorumAction : QuorumAction<TestQuorumAgent, TestOtherQuorumContext>
	{
		protected override void Enter(TestOtherQuorumContext context) { }

		protected override QuorumActionState OnTick(TestOtherQuorumContext context) => QuorumActionState.Running;

		protected override void Exit(TestOtherQuorumContext context) { }
	}
}
