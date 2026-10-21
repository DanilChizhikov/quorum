using System;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestBaseContextQuorumAction : QuorumAction<TestQuorumAgent, TestQuorumContext>
	{
		protected override void Enter(TestQuorumContext context) { }

		protected override QuorumActionState OnTick(TestQuorumContext context) => QuorumActionState.Running;

		protected override void Exit(TestQuorumContext context) { }
	}
}
