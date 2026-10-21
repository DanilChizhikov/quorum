using System;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestOtherContextUtilityAction : UtilityAction<TestUtilityAgent, TestOtherUtilityContext>
	{
		protected override void Enter(TestOtherUtilityContext context) { }

		protected override UtilityActionState OnTick(TestOtherUtilityContext context) => UtilityActionState.Running;

		protected override void Exit(TestOtherUtilityContext context) { }
	}
}
