using System;

namespace DTech.Quorum.Tests.Performance
{
	[Serializable]
	internal sealed class PerfQuorumAction : QuorumAction
	{
		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;

		public override void Exit(IQuorumContext context) { }
	}
}
