using System;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestDerivedContextQuorumAction : QuorumAction<TestQuorumAgent, TestDerivedQuorumContext>
	{
		protected override void Enter(TestDerivedQuorumContext context) { }

		protected override QuorumActionState OnTick(TestDerivedQuorumContext context) => QuorumActionState.Running;

		protected override void Exit(TestDerivedQuorumContext context) { }
	}
}
