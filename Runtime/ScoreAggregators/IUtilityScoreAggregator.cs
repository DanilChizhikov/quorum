using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public interface IUtilityScoreAggregator
	{
		float Aggregate(IReadOnlyList<float> considerationScores);
	}
}
