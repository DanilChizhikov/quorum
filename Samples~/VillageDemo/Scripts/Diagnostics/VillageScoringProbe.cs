using DTech.Blackboard;
using DTech.UtilityAI.Diagnostics;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Builds a villager context for the Scoring Matrix, where no scene and no agent exist.
	/// Works because every villager score is read from the blackboard only (see VillageKeys).
	/// No injector is returned: scoring never touches the world service.
	/// </summary>
	[CreateAssetMenu(menuName = "DTech/Utility AI/Samples/Village Scoring Probe", fileName = "VillageScoringProbe")]
	public sealed class VillageScoringProbe : UtilityScoringProbe
	{
		public override IUtilityContext CreateContext(RuntimeBlackboard blackboard) => new VillageContext(null, blackboard);
	}
}
