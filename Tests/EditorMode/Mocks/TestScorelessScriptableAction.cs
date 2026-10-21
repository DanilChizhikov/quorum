namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestScorelessScriptableAction : ScriptableUtilityAction
	{
		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;

		public override void Exit(IUtilityContext context) { }
	}
}
