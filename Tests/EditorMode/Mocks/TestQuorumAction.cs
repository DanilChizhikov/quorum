using System;
using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestQuorumAction : QuorumAction
	{
		[SerializeField]
		private float _utility = 0f;

		
		public int InitializeCount { get; private set; }
		public int EnterCount { get; private set; }
		public int TickCount { get; private set; }
		public int ExitCount { get; private set; }
		public IQuorumContext LastContext { get; private set; }
		public QuorumActionState TickState { get; set; } = QuorumActionState.Running;
		public float Utility => _utility;
		public Consideration ActionConsideration => Consideration;
		
		public TestQuorumAction() { }
		
		public TestQuorumAction(float utility)
		{
			_utility = utility;
		}
		
		public void SetUtility(float utility)
		{
			_utility = utility;
		}

		
		public override void Initialize(IQuorumContext context)
		{
			InitializeCount++;
			LastContext = context;
		}
		
		public override void Enter(IQuorumContext context)
		{
			EnterCount++;
			LastContext = context;
		}
		
		public override QuorumActionState Tick(IQuorumContext context)
		{
			TickCount++;
			LastContext = context;
			return TickState;
		}
		
		public override float CalculateUtility(IQuorumContext context) => _utility;
		
		public override void Exit(IQuorumContext context)
		{
			ExitCount++;
			LastContext = context;
		}
	}
}
