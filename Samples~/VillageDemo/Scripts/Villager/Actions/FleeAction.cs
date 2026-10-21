using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Run away from the closest wolf until it is far enough. Its score peaks at 1, above every other
	/// villager action, so the periodic re-score in VillagerAgent interrupts anything when a wolf gets close.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Villager/Flee")]
	public sealed class FleeAction : VillageAction<VillagerAgent, VillageContext>
	{
		[SerializeField]
		private float _speed = 3.2f;

		[SerializeField]
		private float _safeDistance = 8f;

		[SerializeField]
		private float _stepDistance = 3f;

		public FleeAction() : base("Flee", new Color(0.95f, 0.2f, 0.2f), 0.75f) { }

		protected override void Enter(VillageContext context) { }

		protected override QuorumActionState OnTick(VillageContext context)
		{
			VillagerAgent agent = context.Agent;
			Vector3 position = agent.transform.position;
			if (!World.TryGetNearestWolf(position, out WolfAgent wolf))
			{
				return QuorumActionState.Completed;
			}

			Vector3 away = position - wolf.transform.position;
			away.y = 0f;
			if (away.magnitude >= _safeDistance)
			{
				return QuorumActionState.Completed;
			}

			Vector3 direction = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.forward;
			Vector3 target = World.ClampToBounds(position + direction * _stepDistance);
			if ((target - position).sqrMagnitude < 1f)
			{
				// Cornered against the edge of the map: slide sideways along it.
				target = World.ClampToBounds(position + Vector3.Cross(direction, Vector3.up) * _stepDistance);
			}

			agent.Mover.MoveTo(target, _speed);
			return QuorumActionState.Running;
		}

		protected override void Exit(VillageContext context) { }
	}
}
