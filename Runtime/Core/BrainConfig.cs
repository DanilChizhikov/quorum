using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class BrainConfig : ScriptableObject
	{
		[SerializeField]
		private BrainUpdateType _updateType = BrainUpdateType.Update;
		
		[SerializeField]
		private SwitchActionType _switchActionType = SwitchActionType.Manual;

		[SerializeField]
		private UtilityAction[] _actions = Array.Empty<UtilityAction>();

		[SerializeField]
		private UtilityConsideration _consideration = default;
		
		[SerializeField]
		private UtilityScoreAggregator _scoreAggregator = default;
		
		public BrainUpdateType UpdateType => _updateType;
		public SwitchActionType SwitchActionType => _switchActionType;
		public IReadOnlyList<UtilityAction> Actions => _actions;
		public UtilityScoreAggregator ScoreAggregator => _scoreAggregator;
		public UtilityConsideration Consideration => _consideration;
	}
}