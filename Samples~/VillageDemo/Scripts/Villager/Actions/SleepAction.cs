using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Go to the closest bed and sleep until fully rested. Its commitment floor keeps the villager in bed
	/// at 60% energy when Work starts scoring higher - a close wolf (Flee) or real hunger (Eat) still wins.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Villager/Sleep")]
	public sealed class SleepAction : VillageAction<VillagerAgent, VillageContext>
	{
		[SerializeField]
		private float _speed = 1.8f;

		[SerializeField]
		private float _restorePerSecond = 0.2f;

		private VillagePoint _bed;

		public SleepAction() : base("Sleep", new Color(0.3f, 0.45f, 0.95f), 0.5f) { }

		protected override void Enter(VillageContext context)
		{
			World.TryGetNearestPoint(VillagePointKind.Bed, context.Agent.transform.position, out _bed);
		}

		protected override QuorumActionState OnTick(VillageContext context)
		{
			if (_bed == null)
			{
				return QuorumActionState.Failed;
			}

			VillagerAgent agent = context.Agent;
			if (!agent.Mover.MoveTo(_bed.Position, _speed, 0.3f))
			{
				return QuorumActionState.Running;
			}

			agent.Energy += _restorePerSecond * Time.deltaTime;
			return agent.Energy >= 1f ? QuorumActionState.Completed : QuorumActionState.Running;
		}

		protected override void Exit(VillageContext context)
		{
			_bed = null;
		}
	}
}
