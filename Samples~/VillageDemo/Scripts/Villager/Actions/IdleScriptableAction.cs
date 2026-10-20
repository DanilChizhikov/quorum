using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// The ScriptableObject flavour of an action, added to the config through a
	/// "Scriptable Action Reference" entry. One asset is shared by every brain and it is never injected,
	/// so it must not keep per-agent state - this one only reads the context it is given.
	/// Prefer plain UtilityAction classes; this path exists for stateless actions and older configs.
	/// </summary>
	[CreateAssetMenu(menuName = "DTech/Utility AI/Samples/Idle Action", fileName = "IdleAction")]
	public sealed class IdleScriptableAction : ScriptableUtilityAction<VillagerAgent, VillageContext>, IVillageActionVisual
	{
		[SerializeField]
		private float _turnSpeed = 60f;

		[SerializeField]
		private float _fallbackScore = 0.05f;

		[SerializeField]
		private Color _tint = new(0.55f, 0.55f, 0.55f);

		public string Label => "Idle";
		public Color Tint => _tint;

		protected override void Enter(VillageContext context) { }

		protected override UtilityActionState OnTick(VillageContext context)
		{
			context.Agent.transform.Rotate(0f, _turnSpeed * Time.deltaTime, 0f);
			return UtilityActionState.Running;
		}

		protected override float CalculateUtility(VillageContext context) =>
			Consideration != null ? Consideration.Evaluate(context) : _fallbackScore;

		protected override void Exit(VillageContext context) { }
	}
}
