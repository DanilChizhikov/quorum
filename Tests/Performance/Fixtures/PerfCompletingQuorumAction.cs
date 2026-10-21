using System;

namespace DTech.Quorum.Tests.Performance
{
	[Serializable]
	internal sealed class PerfCompletingQuorumAction : QuorumAction
	{
		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Completed;

		public override void Exit(IQuorumContext context) { }
	}
}
