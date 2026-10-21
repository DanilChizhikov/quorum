using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	[Serializable]
	[UtilityMenu("Max Utility")]
	public sealed class MaxUtilityScoreAggregator : UtilityScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context)
		{
			// Only a utility above zero counts as a candidate, so a list where nothing scores leaves the
			// brain idle instead of silently winning the first entry. Same cutoff as the random aggregator.
			float max = 0f;
			int maxIndex = -1;
			for (int i = 0; i < actions.Count; i++)
			{
				IUtilityAction action = actions[i];
				float utility = action.CalculateUtility(context);
				if (utility > max)
				{
					max = utility;
					maxIndex = i;
				}
			}
			
			return maxIndex;
		}
	}
}
