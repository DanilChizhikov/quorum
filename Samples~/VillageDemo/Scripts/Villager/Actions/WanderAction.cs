using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Fallback: stroll to a random nearby point. Scored by a low constant so anything useful beats it.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Villager/Wander")]
	public sealed class WanderAction : VillageAction<VillagerAgent, VillageContext>
	{
		[SerializeField]
		private float _speed = 1.4f;

		[SerializeField]
		private float _radius = 4f;

		private Vector3 _target;

		public WanderAction() : base("Wander", new Color(0.85f, 0.85f, 0.85f)) { }

		protected override void Enter(VillageContext context)
		{
			Vector2 offset = Random.insideUnitCircle * _radius;
			_target = World.ClampToBounds(context.Agent.transform.position + new Vector3(offset.x, 0f, offset.y));
		}

		protected override QuorumActionState OnTick(VillageContext context) =>
			context.Agent.Mover.MoveTo(_target, _speed) ? QuorumActionState.Completed : QuorumActionState.Running;

		protected override void Exit(VillageContext context) { }
	}
}
