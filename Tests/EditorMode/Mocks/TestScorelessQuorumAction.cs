using System;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestScorelessQuorumAction : QuorumAction
	{
		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;

		public override void Exit(IQuorumContext context) { }
	}
}
