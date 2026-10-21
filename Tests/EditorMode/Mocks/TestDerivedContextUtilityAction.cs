using System;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestDerivedContextUtilityAction : UtilityAction<TestUtilityAgent, TestDerivedUtilityContext>
	{
		protected override void Enter(TestDerivedUtilityContext context) { }

		protected override UtilityActionState OnTick(TestDerivedUtilityContext context) => UtilityActionState.Running;

		protected override void Exit(TestDerivedUtilityContext context) { }
	}
}
