using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace DTech.UtilityAI
{
	public sealed class RandomUtilityScoreAggregator : UtilityScoreAggregator
	{
		public override int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context)
		{
			int randomIndex = -1;
			float totalUtility = 0f;
			var map = DictionaryPool<IUtilityAction, float>.Get();
			GetUtilityMap(actions, context, map);
			totalUtility = map.Values.Sum();
			float randomUtility = Random.Range(0f, totalUtility);
			for (int i = 0; i < actions.Count; i++)
			{
				IUtilityAction action = actions[i];
				float utility = map[action];
				randomUtility -= utility;
				if (randomUtility <= 0f)
				{
					randomIndex = i;
					break;
				}
			}
			
			map.Clear();
			DictionaryPool<IUtilityAction, float>.Release(map);
			return randomIndex;
		}

		private static void GetUtilityMap(IReadOnlyList<IUtilityAction> actions, IUtilityContext context, Dictionary<IUtilityAction, float> map)
		{
			map.Clear();
			for (int i = 0; i < actions.Count; i++)
			{
				IUtilityAction action = actions[i];
				float utility = action.CalculateUtility(context);
				map[action] = utility;
			}
		}
	}
}