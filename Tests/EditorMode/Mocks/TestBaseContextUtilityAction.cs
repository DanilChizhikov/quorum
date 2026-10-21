using System;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestBaseContextUtilityAction : UtilityAction<TestUtilityAgent, TestUtilityContext>
	{
		protected override void Enter(TestUtilityContext context) { }

		protected override UtilityActionState OnTick(TestUtilityContext context) => UtilityActionState.Running;

		protected override void Exit(TestUtilityContext context) { }
	}
}
