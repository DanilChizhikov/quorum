using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Chase the closest villager. Burns stamina; gives up (Failed) when exhausted or when the prey is gone.
	/// A catch sends the villager home without its wood and fills the wolf's satiation.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Wolf/Hunt")]
	public sealed class HuntAction : VillageAction<WolfAgent, WolfContext>
	{
		[SerializeField]
		private float _speed = 3.6f;

		[SerializeField]
		private float _staminaCostPerSecond = 0.18f;

		[SerializeField]
		private float _catchDistance = 0.7f;

		[SerializeField]
		private float _giveUpDistance = 12f;

		public HuntAction() : base("Hunt", new Color(0.6f, 0.1f, 0.1f)) { }

		protected override void Enter(WolfContext context) { }

		protected override QuorumActionState OnTick(WolfContext context)
		{
			WolfAgent wolf = context.Agent;
			Vector3 position = wolf.transform.position;
			if (!World.TryGetNearestVillager(position, out VillagerAgent prey))
			{
				return QuorumActionState.Failed;
			}

			float distance = Vector3.Distance(position, prey.transform.position);
			if (distance > _giveUpDistance)
			{
				return QuorumActionState.Failed;
			}

			wolf.Stamina -= _staminaCostPerSecond * Time.deltaTime;
			if (wolf.Stamina <= 0f)
			{
				return QuorumActionState.Failed;
			}

			if (!wolf.Mover.MoveTo(prey.transform.position, _speed, _catchDistance))
			{
				return QuorumActionState.Running;
			}

			prey.OnCaught();
			wolf.Satiation = 1f;
			return QuorumActionState.Completed;
		}

		protected override void Exit(WolfContext context) { }
	}
}
