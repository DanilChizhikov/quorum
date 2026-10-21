using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	[Serializable]
	[QuorumMenu("Max Utility")]
	[Preserve]
	public sealed class MaxScoreAggregator : ScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IQuorumAction> actions, IQuorumContext context)
		{
			float max = 0f;
			int maxIndex = -1;
			for (int i = 0; i < actions.Count; i++)
			{
				IQuorumAction action = actions[i];
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
