using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class ScriptableUtilityScoreAggregator : ScriptableObject, IUtilityScoreAggregator
	{
		public abstract float Aggregate(IReadOnlyList<float> considerationScores);
	}
}