using System;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Walk to a random point of the map. Completes on arrival, which is when the wolf brain re-rolls.
	/// </summary>
	[Serializable]
	[UtilityMenu("Village/Wolf/Patrol")]
	public sealed class PatrolAction : VillageAction<WolfAgent, WolfContext>
	{
		[SerializeField]
		private float _speed = 1.8f;

		[SerializeField]
		private float _staminaRestorePerSecond = 0.03f;

		private Vector3 _target;

		public PatrolAction() : base("Patrol", new Color(0.45f, 0.45f, 0.5f)) { }

		protected override void Enter(WolfContext context)
		{
			_target = World.GetRandomPoint();
		}

		protected override UtilityActionState OnTick(WolfContext context)
		{
			context.Agent.Stamina += _staminaRestorePerSecond * Time.deltaTime;
			return context.Agent.Mover.MoveTo(_target, _speed) ? UtilityActionState.Completed : UtilityActionState.Running;
		}

		protected override void Exit(WolfContext context) { }
	}
}
