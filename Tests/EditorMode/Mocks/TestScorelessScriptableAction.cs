namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestScorelessScriptableAction : ScriptableQuorumAction
	{
		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;

		public override void Exit(IQuorumContext context) { }
	}
}
