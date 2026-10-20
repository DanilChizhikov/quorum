using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	/// <summary>
	/// One scoring case: a blackboard snapshot plus the action expected to win on it.
	/// </summary>
	[Serializable]
	public sealed class UtilityScoringCase
	{
		[SerializeField]
		private string _label = string.Empty;

		[SerializeField]
		private BlackboardAsset _blackboard = default;

		[SerializeField]
		private string _expectedActionType = string.Empty;

		/// <summary>
		/// Name shown in the matrix row. Falls back to the blackboard asset name.
		/// </summary>
		public string Label => _label;

		/// <summary>
		/// Blackboard snapshot the context is built from. Null means an empty blackboard.
		/// </summary>
		public BlackboardAsset Blackboard => _blackboard;

		/// <summary>
		/// Optional action type expected to win, as a type name or a picker name. Empty means no expectation.
		/// </summary>
		public string ExpectedActionType => _expectedActionType;

		/// <summary>
		/// Creates an empty case, used by serialization.
		/// </summary>
		public UtilityScoringCase() { }

		/// <summary>
		/// Creates a case from code, used by regression tests.
		/// </summary>
		/// <param name="label">Name shown in the matrix row.</param>
		/// <param name="blackboard">Blackboard snapshot, or null for an empty blackboard.</param>
		/// <param name="expectedActionType">Action type expected to win, or null for no expectation.</param>
		public UtilityScoringCase(string label, BlackboardAsset blackboard, string expectedActionType)
		{
			_label = label;
			_blackboard = blackboard;
			_expectedActionType = expectedActionType;
		}
	}
}
