using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public sealed class UtilityBrain
	{
		private readonly List<UtilityActionDefinition> _actionDefinitions;
		private readonly IUtilityScoreAggregator _scoreAggregator;

		public UtilityBrain(IEnumerable<UtilityActionDefinition> actionDefinitions, IUtilityScoreAggregator scoreAggregator = null)
		{
			if (actionDefinitions == null)
			{
				throw new ArgumentNullException(nameof(actionDefinitions));
			}

			_actionDefinitions = new List<UtilityActionDefinition>(actionDefinitions);
			_scoreAggregator = scoreAggregator ?? new MaxUtilityScoreAggregator();
		}

		public IReadOnlyList<UtilityActionDefinition> ActionDefinitions => _actionDefinitions;

		public UtilityDecision<IUtilityAction> Decide(IUtilityContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			int bestIndex = -1;
			float bestScore = float.NegativeInfinity;

			for (int i = 0; i < _actionDefinitions.Count; i++)
			{
				UtilityActionDefinition definition = _actionDefinitions[i];
				float score = EvaluateActionScore(definition, context);
				if (score > bestScore)
				{
					bestScore = score;
					bestIndex = i;
				}
			}

			if (bestIndex < 0)
			{
				return UtilityDecision<IUtilityAction>.None;
			}

			UtilityActionDefinition bestDefinition = _actionDefinitions[bestIndex];
			return new UtilityDecision<IUtilityAction>(bestDefinition.Action, bestScore, bestIndex);
		}

		private float EvaluateActionScore(UtilityActionDefinition definition, IUtilityContext context)
		{
			IReadOnlyList<IUtilityConsideration> considerations = definition.Considerations;
			if (considerations.Count == 0)
			{
				return _scoreAggregator.Aggregate(Array.Empty<float>());
			}

			float[] scores = new float[considerations.Count];
			for (int i = 0; i < considerations.Count; i++)
			{
				scores[i] = considerations[i].Evaluate(context);
			}

			return _scoreAggregator.Aggregate(scores);
		}
	}
}
