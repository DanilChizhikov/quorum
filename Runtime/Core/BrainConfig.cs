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

		[SerializeReference]
		private UtilityAction[] _actionEntries = Array.Empty<UtilityAction>();

		[SerializeReference]
		private UtilityConsideration _consideration = default;
		
		[SerializeReference]
		private UtilityScoreAggregator _scoreAggregator = new MaxUtilityScoreAggregator();
		
		[SerializeField, HideInInspector]
		private ScriptableUtilityAction[] _actions = Array.Empty<ScriptableUtilityAction>();
		
		public BrainUpdateType UpdateType => _updateType;
		public SwitchActionType SwitchActionType => _switchActionType;
		public IReadOnlyList<UtilityAction> Actions => _actionEntries;
		public UtilityScoreAggregator ScoreAggregator => _scoreAggregator;
		public UtilityConsideration Consideration => _consideration;
		
		internal bool HasLegacyActions => _actions is { Length: > 0 };
		
		internal void SetActions(UtilityAction[] actions)
		{
			_actionEntries = actions ?? Array.Empty<UtilityAction>();
		}
		
		internal void SetScoreAggregator(UtilityScoreAggregator scoreAggregator)
		{
			_scoreAggregator = scoreAggregator;
		}
		
		internal void UpgradeLegacyActions()
		{
			if (!HasLegacyActions)
			{
				return;
			}

			var entries = new List<UtilityAction>(_actionEntries ?? Array.Empty<UtilityAction>());
			for (int i = 0; i < _actions.Length; i++)
			{
				ScriptableUtilityAction action = _actions[i];
				if (action == null)
				{
					continue;
				}
				
				entries.Add(new ScriptableUtilityActionReference(action));
			}

			_actionEntries = entries.ToArray();
			_actions = Array.Empty<ScriptableUtilityAction>();
		}
	}
}
