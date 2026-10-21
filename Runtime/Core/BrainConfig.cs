using System;
using System.Collections.Generic;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum
{
	public abstract class BrainConfig : ScriptableObject
	{
		[SerializeField]
		private BrainUpdateType _updateType = BrainUpdateType.Update;
		
		[SerializeField]
		private SwitchActionType _switchActionType = SwitchActionType.OnActionFinished;

		[SerializeReference]
		private QuorumAction[] _actionEntries = Array.Empty<QuorumAction>();
		
		[SerializeReference]
		private ScoreAggregator _scoreAggregator = new MaxScoreAggregator();
		
		[SerializeField]
		private BlackboardAsset _blackboardSchema = default;
		
		public BrainUpdateType UpdateType => _updateType;
		public SwitchActionType SwitchActionType => _switchActionType;
		public IReadOnlyList<QuorumAction> Actions => _actionEntries;
		public ScoreAggregator ScoreAggregator => _scoreAggregator;
		public BlackboardAsset BlackboardSchema => _blackboardSchema;
		
		internal void SetActions(QuorumAction[] actions)
		{
			_actionEntries = actions ?? Array.Empty<QuorumAction>();
		}
		
		internal void SetScoreAggregator(ScoreAggregator scoreAggregator)
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
