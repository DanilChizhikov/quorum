namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfScriptableAction : ScriptableUtilityAction
	{
		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;

		public override void Exit(IUtilityContext context) { }
	}
}
