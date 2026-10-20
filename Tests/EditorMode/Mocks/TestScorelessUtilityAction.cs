using System;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestScorelessUtilityAction : UtilityAction
	{
		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;

		public override void Exit(IUtilityContext context) { }
	}
}
