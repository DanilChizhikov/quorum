using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Lie down until stamina is back.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Wolf/Rest")]
	public sealed class RestAction : VillageAction<WolfAgent, WolfContext>
	{
		[SerializeField]
		private float _restorePerSecond = 0.25f;

		public RestAction() : base("Rest", new Color(0.25f, 0.2f, 0.35f)) { }

		protected override void Enter(WolfContext context) { }

		protected override QuorumActionState OnTick(WolfContext context)
		{
			context.Agent.Stamina += _restorePerSecond * Time.deltaTime;
			return context.Agent.Stamina >= 1f ? QuorumActionState.Completed : QuorumActionState.Running;
		}

		protected override void Exit(WolfContext context) { }
	}
}
