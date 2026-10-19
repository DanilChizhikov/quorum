using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class TestScriptableAction : ScriptableUtilityAction
	{
		private float _utility;
		
		public int InitializeCount { get; private set; }
		public int EnterCount { get; private set; }
		public int TickCount { get; private set; }
		public int ExitCount { get; private set; }
		public UtilityActionState TickState { get; set; } = UtilityActionState.Running;
		
		public void SetUtility(float utility)
		{
			_utility = utility;
		}
		
		public override void Initialize(IUtilityContext context)
		{
			InitializeCount++;
		}
		
		public override void Enter(IUtilityContext context)
		{
			EnterCount++;
		}
		
		public override UtilityActionState Tick(IUtilityContext context)
		{
			TickCount++;
			return TickState;
		}
		
		public override float CalculateUtility(IUtilityContext context) => _utility;
		
		public override void Exit(IUtilityContext context)
		{
			ExitCount++;
		}
	}
}
