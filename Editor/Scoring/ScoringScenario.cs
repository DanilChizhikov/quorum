using System;
using System.Collections.Generic;
using DTech.Quorum.Diagnostics;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	/// <summary>
	/// Set of scoring cases run against one BrainConfig by the Scoring Matrix window.
	/// </summary>
	[CreateAssetMenu(menuName = "DTech/Quorum/Scoring Scenario", fileName = "ScoringScenario")]
	public sealed class ScoringScenario : ScriptableObject
	{
		[SerializeField]
		private ScoringProbe _probe = default;

		[SerializeField]
		private BrainConfig _config = default;

		[SerializeField]
		private ScoringCase[] _cases = Array.Empty<ScoringCase>();

		/// <summary>
		/// Probe that builds the context for every case.
		/// </summary>
		public ScoringProbe Probe => _probe;

		/// <summary>
		/// Config whose actions are scored.
		/// </summary>
		public BrainConfig Config => _config;

		/// <summary>
		/// Cases run as matrix rows.
		/// </summary>
		public IReadOnlyList<ScoringCase> Cases => _cases;
	}
}
