using System;
using System.Collections.Generic;
using DTech.UtilityAI.Diagnostics;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	/// <summary>
	/// Set of scoring cases run against one BrainConfig by the Scoring Matrix window.
	/// </summary>
	[CreateAssetMenu(menuName = "DTech/Utility AI/Scoring Scenario", fileName = "UtilityScoringScenario")]
	public sealed class UtilityScoringScenario : ScriptableObject
	{
		[SerializeField]
		private UtilityScoringProbe _probe = default;

		[SerializeField]
		private BrainConfig _config = default;

		[SerializeField]
		private UtilityScoringCase[] _cases = Array.Empty<UtilityScoringCase>();

		/// <summary>
		/// Probe that builds the context for every case.
		/// </summary>
		public UtilityScoringProbe Probe => _probe;

		/// <summary>
		/// Config whose actions are scored.
		/// </summary>
		public BrainConfig Config => _config;

		/// <summary>
		/// Cases run as matrix rows.
		/// </summary>
		public IReadOnlyList<UtilityScoringCase> Cases => _cases;
	}
}
