using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Lets the HUD tint an agent with the colour of its current action.
	/// </summary>
	public interface IVillageActionVisual
	{
		string Label { get; }
		Color Tint { get; }
	}

	/// <summary>
	/// Shared base of the sample actions: a serialized label/tint for the HUD, the injected world and
	/// a commitment floor. Every brain gets its own clone of the config, so fields set in
	/// <c>Enter</c>/<c>OnTick</c> are per-agent state and never leak between villagers.
	/// </summary>
	/// <remarks>
	/// Commitment (hysteresis): while the action is running its score never drops below the floor.
	/// Without it two actions with close scores flip every re-score - Flee runs away, the wolf distance
	/// grows, Work wins, the villager walks back, Flee wins again. The floor only applies while running,
	/// so the Scoring Matrix (which never enters actions) still shows the raw scores.
	/// </remarks>
	[Serializable]
	public abstract class VillageAction<TAgent, TContext> : QuorumAction<TAgent, TContext>,
		IVillageActionVisual, IVillageWorldConsumer
		where TAgent : IQuorumAgent
		where TContext : QuorumContext<TAgent>
	{
		[SerializeField]
		private string _label = string.Empty;

		[SerializeField]
		private Color _tint = Color.white;

		[SerializeField, Tooltip("While the action is running its score never drops below this value. 0 = off.")]
		private float _commitmentFloor = 0f;

		private bool _isRunning;

		public string Label => string.IsNullOrEmpty(_label) ? GetType().Name : _label;
		public Color Tint => _tint;

		protected VillageWorld World { get; private set; }

		protected VillageAction(string label, Color tint, float commitmentFloor = 0f)
		{
			_label = label;
			_tint = tint;
			_commitmentFloor = commitmentFloor;
		}

		public void Construct(VillageWorld world)
		{
			World = world;
		}

		public override void Enter(IQuorumContext context)
		{
			_isRunning = true;
			base.Enter(context);
		}

		public override QuorumActionState Tick(IQuorumContext context)
		{
			QuorumActionState state = base.Tick(context);
			_isRunning = state == QuorumActionState.Running;
			return state;
		}

		public override float CalculateUtility(IQuorumContext context)
		{
			float score = base.CalculateUtility(context);
			return _isRunning ? Mathf.Max(score, _commitmentFloor) : score;
		}

		public override void Exit(IQuorumContext context)
		{
			_isRunning = false;
			base.Exit(context);
		}
	}

	internal static class VillageActionNames
	{
		public static bool TryGetVisual(IQuorumAction action, out IVillageActionVisual visual)
		{
			visual = action switch
			{
				IVillageActionVisual actionVisual => actionVisual,
				ScriptableQuorumActionReference { Action: IVillageActionVisual assetVisual } => assetVisual,
				_ => null,
			};

			return visual != null;
		}
	}
}
