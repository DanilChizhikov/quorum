using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace DTech.UtilityAI
{
	[Serializable]
	[UtilityMenu("Random Weighted Utility")]
	public sealed class RandomUtilityScoreAggregator : UtilityScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context)
		{
			List<float> utilities = ListPool<float>.Get();
			float totalUtility = GetUtilities(actions, context, utilities);
			if (totalUtility <= 0f)
			{
				utilities.Clear();
				ListPool<float>.Release(utilities);
				return -1;
			}
			
			int randomIndex = -1;
			float randomUtility = Random.Range(0f, totalUtility);
			for (int i = 0; i < utilities.Count; i++)
			{
				randomUtility -= utilities[i];
				if (randomUtility <= 0f)
				{
					randomIndex = i;
					break;
				}
			}
			
			utilities.Clear();
			ListPool<float>.Release(utilities);
			return randomIndex;
		}

		private static float GetUtilities(IReadOnlyList<IUtilityAction> actions, IUtilityContext context, List<float> utilities)
		{
			utilities.Clear();
			float totalUtility = 0f;
			for (int i = 0; i < actions.Count; i++)
			{
				IUtilityAction action = actions[i];
				float utility = Mathf.Max(0f, action.CalculateUtility(context));
				utilities.Add(utility);
				totalUtility += utility;
			}

			return totalUtility;
		}
	}
}
