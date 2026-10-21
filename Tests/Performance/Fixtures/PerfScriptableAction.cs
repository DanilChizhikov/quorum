namespace DTech.Quorum.Tests.Performance
{
	internal sealed class PerfScriptableAction : ScriptableQuorumAction
	{
		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;

		public override void Exit(IQuorumContext context) { }
	}
}
