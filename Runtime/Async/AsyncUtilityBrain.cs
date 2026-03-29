using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DTech.UtilityAI
{
	public sealed class AsyncUtilityBrain
	{
		private readonly List<UtilityAsyncActionDefinition> _actionDefinitions;
		private readonly IUtilityScoreAggregator _scoreAggregator;

		public AsyncUtilityBrain(IEnumerable<UtilityAsyncActionDefinition> actionDefinitions, IUtilityScoreAggregator scoreAggregator = null)
		{
			if (actionDefinitions == null)
			{
				throw new ArgumentNullException(nameof(actionDefinitions));
			}

			_actionDefinitions = new List<UtilityAsyncActionDefinition>(actionDefinitions);
			_scoreAggregator = scoreAggregator ?? new MaxUtilityScoreAggregator();
		}

		public IReadOnlyList<UtilityAsyncActionDefinition> ActionDefinitions => _actionDefinitions;

		public async ValueTask<UtilityDecision<IAsyncUtilityAction>> DecideAsync(IUtilityContext context, CancellationToken token)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			int bestIndex = -1;
			float bestScore = float.NegativeInfinity;

			for (int i = 0; i < _actionDefinitions.Count; i++)
			{
				token.ThrowIfCancellationRequested();

				UtilityAsyncActionDefinition definition = _actionDefinitions[i];
				float score = await EvaluateActionScoreAsync(definition, context, token);
				if (score > bestScore)
				{
					bestScore = score;
					bestIndex = i;
				}
			}

			if (bestIndex < 0)
			{
				return UtilityDecision<IAsyncUtilityAction>.None;
			}

			UtilityAsyncActionDefinition bestDefinition = _actionDefinitions[bestIndex];
			return new UtilityDecision<IAsyncUtilityAction>(bestDefinition.Action, bestScore, bestIndex);
		}

		private async ValueTask<float> EvaluateActionScoreAsync(
			UtilityAsyncActionDefinition definition,
			IUtilityContext context,
			CancellationToken token)
		{
			IReadOnlyList<IAsyncUtilityConsideration> considerations = definition.Considerations;
			if (considerations.Count == 0)
			{
				return _scoreAggregator.Aggregate(Array.Empty<float>());
			}

			float[] scores = new float[considerations.Count];
			for (int i = 0; i < considerations.Count; i++)
			{
				token.ThrowIfCancellationRequested();
				scores[i] = await considerations[i].EvaluateAsync(context, token);
			}

			return _scoreAggregator.Aggregate(scores);
		}
	}
}
