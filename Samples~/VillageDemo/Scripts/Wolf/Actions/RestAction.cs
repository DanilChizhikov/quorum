using System;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Lie down until stamina is back.
	/// </summary>
	[Serializable]
	[UtilityMenu("Village/Wolf/Rest")]
	public sealed class RestAction : VillageAction<WolfAgent, WolfContext>
	{
		[SerializeField]
		private float _restorePerSecond = 0.25f;

		public RestAction() : base("Rest", new Color(0.25f, 0.2f, 0.35f)) { }

		protected override void Enter(WolfContext context) { }

		protected override UtilityActionState OnTick(WolfContext context)
		{
			context.Agent.Stamina += _restorePerSecond * Time.deltaTime;
			return context.Agent.Stamina >= 1f ? UtilityActionState.Completed : UtilityActionState.Running;
		}

		protected override void Exit(WolfContext context) { }
	}
}
