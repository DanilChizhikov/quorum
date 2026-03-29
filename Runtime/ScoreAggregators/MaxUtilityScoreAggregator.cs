using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public sealed class MaxUtilityScoreAggregator : IUtilityScoreAggregator
	{
		public float Aggregate(IReadOnlyList<float> considerationScores)
		{
			if (considerationScores == null || considerationScores.Count == 0)
			{
				return 0f;
			}

			float max = float.NegativeInfinity;
			for (int i = 0; i < considerationScores.Count; i++)
			{
				if (considerationScores[i] > max)
				{
					max = considerationScores[i];
				}
			}

			return max;
		}
	}
}
