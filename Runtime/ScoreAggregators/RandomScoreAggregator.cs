using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace DTech.Quorum
{
	[Serializable]
	[QuorumMenu("Random Weighted Utility")]
	[Preserve]
	public sealed class RandomScoreAggregator : ScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IQuorumAction> actions, IQuorumContext context)
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
			return randomIndex >= 0 ? randomIndex : lastPositiveIndex;
		}

		private static float GetUtilities(IReadOnlyList<IQuorumAction> actions, IQuorumContext context, List<float> utilities)
		{
			utilities.Clear();
			float totalUtility = 0f;
			for (int i = 0; i < actions.Count; i++)
			{
				IQuorumAction action = actions[i];
				float utility = Mathf.Max(0f, action.CalculateUtility(context));
				utilities.Add(utility);
				totalUtility += utility;
			}

			return totalUtility;
		}
	}
}
