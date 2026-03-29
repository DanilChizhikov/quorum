using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public sealed class MaxUtilityScoreAggregator : UtilityScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context)
		{
			float max = float.NegativeInfinity;
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
