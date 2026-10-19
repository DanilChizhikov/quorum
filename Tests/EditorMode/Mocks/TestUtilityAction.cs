using System;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestUtilityAction : UtilityAction
	{
		[SerializeField]
		private float _utility = 0f;

		
		public int InitializeCount { get; private set; }
		public int EnterCount { get; private set; }
		public int TickCount { get; private set; }
		public int ExitCount { get; private set; }
		public IUtilityContext LastContext { get; private set; }
		public UtilityActionState TickState { get; set; } = UtilityActionState.Running;
		public float Utility => _utility;
		public UtilityConsideration ActionConsideration => Consideration;
		
		public TestUtilityAction() { }
		
		public TestUtilityAction(float utility)
		{
			_utility = utility;
		}
		
		public void SetUtility(float utility)
		{
			_utility = utility;
		}

		
		public override void Initialize(IUtilityContext context)
		{
			InitializeCount++;
			LastContext = context;
		}
		
		public override void Enter(IUtilityContext context)
		{
			EnterCount++;
			LastContext = context;
		}
		
		public override UtilityActionState Tick(IUtilityContext context)
		{
			TickCount++;
			LastContext = context;
			return TickState;
		}
		
		public override float CalculateUtility(IUtilityContext context) => _utility;
		
		public override void Exit(IUtilityContext context)
		{
			ExitCount++;
			LastContext = context;
		}
	}
}
