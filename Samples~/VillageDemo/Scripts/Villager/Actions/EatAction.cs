using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Walk to the closest bush and eat it. Fails when another villager eats it first -
	/// with SwitchActionType.OnActionFinished a failed action makes the brain pick again right away.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Villager/Eat")]
	public sealed class EatAction : VillageAction<VillagerAgent, VillageContext>
	{
		[SerializeField]
		private float _speed = 2f;

		private FoodSource _food;

		public EatAction() : base("Eat", new Color(0.35f, 0.85f, 0.3f), 0.4f) { }

		protected override void Enter(VillageContext context)
		{
			World.TryGetNearestFood(context.Agent.transform.position, out _food);
		}

		protected override QuorumActionState OnTick(VillageContext context)
		{
			if (_food == null)
			{
				return QuorumActionState.Failed;
			}

			if (!context.Agent.Mover.MoveTo(_food.Position, _speed, 0.6f))
			{
				return QuorumActionState.Running;
			}

			if (!World.TryConsume(_food))
			{
				return QuorumActionState.Failed;
			}

			context.Agent.Hunger = 0f;
			return QuorumActionState.Completed;
		}

		protected override void Exit(VillageContext context)
		{
			_food = null;
		}
	}
}
