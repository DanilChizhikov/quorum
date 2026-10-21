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
			int lastPositiveIndex = -1;
			float randomUtility = Random.Range(0f, totalUtility);
			for (int i = 0; i < utilities.Count; i++)
			{
				// A zero weight must never win, and Random.Range includes both ends for floats.
				if (utilities[i] <= 0f)
				{
					continue;
				}

				lastPositiveIndex = i;
				randomUtility -= utilities[i];
				if (randomUtility <= 0f)
				{
					randomIndex = i;
					break;
				}
			}
			
			utilities.Clear();
			ListPool<float>.Release(utilities);
			// Rounding error over many weights can leave a remainder after the last subtraction.
			return randomIndex >= 0 ? randomIndex : lastPositiveIndex;
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
