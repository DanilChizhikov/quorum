using System;
using System.Collections.Generic;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class BrainConfig : ScriptableObject
	{
		[SerializeField]
		private BrainUpdateType _updateType = BrainUpdateType.Update;
		
		[SerializeField]
		private SwitchActionType _switchActionType = SwitchActionType.Manual;

		[SerializeReference]
		private UtilityAction[] _actionEntries = Array.Empty<UtilityAction>();
		
		[SerializeReference]
		private UtilityScoreAggregator _scoreAggregator = new MaxUtilityScoreAggregator();
		
		[SerializeField]
		private BlackboardAsset _blackboardSchema = default;
		
		public BrainUpdateType UpdateType => _updateType;
		public SwitchActionType SwitchActionType => _switchActionType;
		public IReadOnlyList<UtilityAction> Actions => _actionEntries;
		public UtilityScoreAggregator ScoreAggregator => _scoreAggregator;
		public BlackboardAsset BlackboardSchema => _blackboardSchema;
		
		internal void SetActions(UtilityAction[] actions)
		{
			_actionEntries = actions ?? Array.Empty<UtilityAction>();
		}
		
		internal void SetScoreAggregator(UtilityScoreAggregator scoreAggregator)
		{
			_scoreAggregator = scoreAggregator;
		}
		
		#if UNITY_EDITOR
		internal void SetBlackboardSchema(BlackboardAsset schema)
		{
			_blackboardSchema = schema;
		}
		#endif
	}
}
