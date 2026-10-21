using System;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfUtilityAction : UtilityAction
	{
		public override void Enter(IUtilityContext context) { }

		public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;

		public override void Exit(IUtilityContext context) { }
	}
}
