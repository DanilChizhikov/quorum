using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestSharedFormulaScriptableAction : ScriptableQuorumAction
	{
		[SerializeReference]
		private Consideration _fallbackConsideration = default;

		public void SetFallbackConsideration(Consideration consideration)
		{
			_fallbackConsideration = consideration;
		}

		public override void Enter(IQuorumContext context) { }

		public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;

		public override float CalculateUtility(IQuorumContext context) =>
			_fallbackConsideration != null ? _fallbackConsideration.Evaluate(context) : 0f;

		public override void Exit(IQuorumContext context) { }
	}
}
