using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class TestScriptableAction : ScriptableQuorumAction
	{
		private float _utility;
		
		public int InitializeCount { get; private set; }
		public int EnterCount { get; private set; }
		public int TickCount { get; private set; }
		public int ExitCount { get; private set; }
		public QuorumActionState TickState { get; set; } = QuorumActionState.Running;
		
		public void SetUtility(float utility)
		{
			_utility = utility;
		}
		
		public override void Initialize(IQuorumContext context)
		{
			InitializeCount++;
		}
		
		public override void Enter(IQuorumContext context)
		{
			EnterCount++;
		}
		
		public override QuorumActionState Tick(IQuorumContext context)
		{
			TickCount++;
			return TickState;
		}
		
		public override float CalculateUtility(IQuorumContext context) => _utility;
		
		public override void Exit(IQuorumContext context)
		{
			ExitCount++;
		}
	}
}
