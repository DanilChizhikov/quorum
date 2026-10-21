using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	[Serializable]
	[UtilityMenu("Max Utility")]
	[Preserve]
	public sealed class MaxUtilityScoreAggregator : UtilityScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context)
		{
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
