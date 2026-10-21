using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestSharedFormulaScriptableAction : ScriptableUtilityAction
	{
		[SerializeReference]
		private UtilityConsideration _fallbackConsideration = default;

		public void SetFallbackConsideration(UtilityConsideration consideration)
		{
			_fallbackConsideration = consideration;
		}

		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;

		public override float CalculateUtility(IUtilityContext context) =>
			_fallbackConsideration != null ? _fallbackConsideration.Evaluate(context) : 0f;

		public override void Exit(IUtilityContext context) { }
	}
}
