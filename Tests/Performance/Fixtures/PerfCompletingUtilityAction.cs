using System;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfCompletingUtilityAction : UtilityAction
	{
		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Completed;

		public override void Exit(IUtilityContext context) { }
	}
}
